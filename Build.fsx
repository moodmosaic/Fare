#r @"build/tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open Fake.AppVeyor
open Fake.DotNetCli
open System.Text.RegularExpressions

let buildDir = getBuildParamOrDefault "BuildDir" "build"
let buildToolsDir = buildDir </> "tools"
let nuGetOutputFolder = buildDir </> "NuGetPackages"
let nuGetPackages = !! (nuGetOutputFolder </> "*.nupkg" )
                    // Skip symbol packages because NuGet publish symbols automatically when package is published.
                    -- (nuGetOutputFolder </> "*.symbols.nupkg")
let solutionToBuild = "Src/Fare.sln"
let testProjectDir = "Src/Fare.IntegrationTests"
let configuration = getBuildParamOrDefault "BuildConfiguration" "Release"

type BuildVersionCalculationSource = { major: int; minor: int; revision: int; preSuffix: string; 
                                       commitsNum: int; sha: string; buildNumber: int }
let getVersionSourceFromGit buildNumber =
    // The --fist-parent flag is required to correctly work for vNext branch.
    // Example of output for a release tag: v3.50.2-288-g64fd5c5b, for a prerelease tag: v3.50.2-alpha1-288-g64fd5c5b.
    let desc = Git.CommandHelper.runSimpleGitCommand "" "describe --tags --long --abbrev=40 --first-parent --match=v*"

    // Previously repository contained a few broken tags like "v.3.21.1". They were removed, but could still exist
    // in forks. We handle them as well to not fail on such repositories.
    let result = Regex.Match(desc,
                             @"^v(\.)?(?<maj>\d+)\.(?<min>\d+)\.(?<rev>\d+)(?<pre>-\w+\d*)?-(?<num>\d+)-g(?<sha>[a-z0-9]+)$",
                             RegexOptions.IgnoreCase)
                      .Groups

    let getMatch (name:string) = result.[name].Value

    { major = getMatch "maj" |> int
      minor = getMatch "min" |> int
      revision = getMatch "rev" |> int
      preSuffix = getMatch "pre"
      commitsNum = getMatch "num" |> int
      sha = getMatch "sha"
      buildNumber = buildNumber
    }

type BuildVersionInfo = { assemblyVersion:string; fileVersion:string; infoVersion:string; nugetVersion:string; 
                          source: Option<BuildVersionCalculationSource> }
let calculateVersion source =
    let s = source
    let (major, minor, revision, preReleaseSuffix, commitsNum, sha, buildNumber) =
        (s.major, s.minor, s.revision, s.preSuffix, s.commitsNum, s.sha, s.buildNumber)

    let assemblyVersion = sprintf "%d.%d.0.0" major minor
    let fileVersion = sprintf "%d.%d.%d.%d" major minor revision buildNumber
    
    // If number of commits since last tag is greater than zero, we append another identifier with number of commits.
    // The produced version is larger than the last tag version.
    // If we are on a tag, we use version without modification.
    // Examples of output: 3.50.2.1, 3.50.2.215, 3.50.1-rc1.3, 3.50.1-rc3.35
    let nugetVersion = match commitsNum with
                       | 0 -> sprintf "%d.%d.%d%s" major minor revision preReleaseSuffix
                       | _ -> sprintf "%d.%d.%d%s.%d" major minor revision preReleaseSuffix commitsNum

    let infoVersion = match commitsNum with
                      | 0 -> nugetVersion
                      | _ -> sprintf "%s-%s" nugetVersion sha

    { assemblyVersion=assemblyVersion; fileVersion=fileVersion; infoVersion=infoVersion; nugetVersion=nugetVersion; 
      source = Some source }

// Calculate version that should be used for the build. Define globally as data might be required by multiple targets.
// Please never name the build parameter with version as "Version" - it might be consumed by the MSBuild, override 
// the defined properties and break some tasks (e.g. NuGet restore).
let buildVersion = match getBuildParamOrDefault "BuildVersion" "git" with
                   | "git"       -> getBuildParamOrDefault "BuildNumber" "0"
                                    |> int
                                    |> getVersionSourceFromGit
                                    |> calculateVersion

                   | assemblyVer -> { assemblyVersion = assemblyVer
                                      fileVersion = getBuildParamOrDefault "BuildFileVersion" assemblyVer
                                      infoVersion = getBuildParamOrDefault "BuildInfoVersion" assemblyVer
                                      nugetVersion = getBuildParamOrDefault "BuildNugetVersion" assemblyVer
                                      source = None }
                                      
let runDotNet command configuration properties =
    let stringProps = properties
                      @ [ "AssemblyVersion", buildVersion.assemblyVersion
                          "FileVersion", buildVersion.fileVersion
                          "InformationalVersion", buildVersion.infoVersion
                          "PackageVersion", buildVersion.nugetVersion ]
                      |> Seq.map(fun (name, value) -> sprintf "/p:%s=%s" name value )
                      |> String.concat " "
                      
    DotNetCli.RunCommand id
                         (sprintf "%s %s --configuration %s %s" command solutionToBuild configuration stringProps)

Target "Verify" (fun _ -> runDotNet "build" configuration [])
Target "Build" (fun _ -> runDotNet "build" configuration [])

Target "Test" (fun _ ->
    DotNetCli.RunCommand (fun p -> { p with WorkingDir = testProjectDir })
                         (sprintf "test --configuration %s" configuration)
)

Target "CleanNuGetPackages" (fun _ ->
    CleanDir nuGetOutputFolder
)

Target "NuGetPack" (fun _ ->
    // Pack projects using MSBuild.
    runDotNet "pack" configuration [ "IncludeSource", "true"
                                     "IncludeSymbols", "true"
                                     "PackageOutputPath", FullName nuGetOutputFolder ]
)

let publishPackagesWithSymbols packageFeed symbolFeed accessKey =
    nuGetPackages
    |> Seq.map (fun pkg ->
        let meta = GetMetaDataFromPackageFile pkg
        meta.Id, meta.Version
    )
    |> Seq.iter (fun (id, version) -> NuGetPublish (fun p -> { p with Project = id
                                                                      Version = version
                                                                      OutputPath = nuGetOutputFolder
                                                                      PublishUrl = packageFeed
                                                                      AccessKey = accessKey
                                                                      SymbolPublishUrl = symbolFeed
                                                                      SymbolAccessKey = accessKey
                                                                      WorkingDir = nuGetOutputFolder
                                                                      ToolPath = buildToolsDir </> "nuget.exe" }))

Target "PublishNuGetPublic" (fun _ ->
    let feed = "https://www.nuget.org/api/v2/package"
    let key = getBuildParam "NuGetPublicKey"

    publishPackagesWithSymbols feed "" key
)

Target "CompleteBuild"   DoNothing

"CleanNuGetPackages" ==> "NuGetPack"
"Verify"             ==> "NuGetPack"
"Test"               ==> "NuGetPack"

"NuGetPack" ==> "CompleteBuild"

"NuGetPack" ==> "PublishNuGetPublic"

// ==============================================
// ================== AppVeyor ==================
// ==============================================

// Add a helper to identify whether current trigger is PR.
type AppVeyorEnvironment with
    static member IsPullRequest = isNotNullOrEmpty AppVeyorEnvironment.PullRequestNumber

type AppVeyorTrigger = SemVerTag | CustomTag | PR | Unknown
let anAppVeyorTrigger =
    let tag = if AppVeyorEnvironment.RepoTag then Some AppVeyorEnvironment.RepoTagName else None
    let isPR = AppVeyorEnvironment.IsPullRequest
    let branch = if isNotNullOrEmpty AppVeyorEnvironment.RepoBranch then Some AppVeyorEnvironment.RepoBranch else None

    match tag, isPR, branch with
    | Some t, _, _ when "v\d+.*" >** t -> SemVerTag
    | Some _, _, _                     -> CustomTag
    | _, true, _                       -> PR
    | _                                -> Unknown

// Print state info at the very beginning.
if buildServer = BuildServer.AppVeyor 
   then logfn "[AppVeyor state] Is tag: %b, tag name: '%s', is PR: %b, branch name: '%s', trigger: %A"
              AppVeyorEnvironment.RepoTag 
              AppVeyorEnvironment.RepoTagName 
              AppVeyorEnvironment.IsPullRequest
              AppVeyorEnvironment.RepoBranch
              anAppVeyorTrigger

Target "AppVeyor" (fun _ ->
    // Artifacts might be deployable, so we update build version to find them later by file version.
    if not AppVeyorEnvironment.IsPullRequest then UpdateBuildVersion buildVersion.fileVersion
)

// Add logic to resolve action based on current trigger info.
dependency "AppVeyor" <| match anAppVeyorTrigger with
                         | SemVerTag                -> "PublishNuGetPublic"
                         | PR | CustomTag | Unknown -> "CompleteBuild"

// ========= ENTRY POINT =========
RunTargetOrDefault "CompleteBuild"