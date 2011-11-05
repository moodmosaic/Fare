/*
 * Copyright 2003-2004 Sun Microsystems, Inc.  All Rights Reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.  Sun designates this
 * particular file as subject to the "Classpath" exception as provided
 * by Sun in the LICENSE file that accompanied this code.
 *
 * This code is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
 * version 2 for more details (a copy is included in the LICENSE file that
 * accompanied this code).
 *
 * You should have received a copy of the GNU General Public License version
 * 2 along with this work; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 *
 * Please contact Sun Microsystems, Inc., 4150 Network Circle, Santa Clara,
 * CA 95054 USA or visit www.sun.com if you need additional information or
 * have any questions.
 */

namespace NAutomaton
{
    /// <summary> 
    /// The result of a match operation.
    /// <p>This interface contains query methods used to determine the
    /// results of a match against a regular expression. The match boundaries,
    /// groups and group boundaries can be seen but not modified through
    /// </p>
    /// a <code>MatchResult</code>.
    /// @author  Michael McCloskey </summary>
    public interface IMatchResult
    {
        /// <summary>
        /// Returns the start index of the match.
        /// </summary>
        /// <returns> 
        /// The index of the first character matched
        /// </returns>
        int Start();

        /// <summary>
        /// Returns the start index of the subsequence captured by the given group
        /// during this match.
        /// <p> <a href="Pattern.html#cg">Capturing groups</a> are indexed from left
        /// to right, starting at one.  Group zero denotes the entire pattern, so
        /// the expression <i>m.</i><tt>start(0)</tt> is equivalent to
        /// <i>m.</i><tt>start()</tt>.  </p>
        /// </summary>
        /// <param name="group">
        /// The index of a capturing group in this matcher's pattern
        /// </param>
        /// <returns>  The index of the first character captured by the group,
        /// or <tt>-1</tt> if the match was successful but the group
        /// itself did not match anything
        /// </returns>
        int Start(int group);

        /// <summary>
        /// Returns the offset after the last character matched.
        /// </summary> 
        /// <returns>  
        /// The offset after the last character matched
        /// </returns>
        int End();

        /// <summary>
        /// Returns the offset after the last character of the subsequence
        /// captured by the given group during this match.
        /// <p> <a href="Pattern.html#cg">Capturing groups</a> are indexed from left
        /// to right, starting at one.  Group zero denotes the entire pattern, so
        /// the expression <i>m.</i><tt>end(0)</tt> is equivalent to
        /// <i>m.</i><tt>end()</tt>.  </p>
        /// </summary>
        /// <param name="group">
        /// The index of a capturing group in this matcher's pattern
        /// </param>
        /// <returns>  The offset after the last character captured by the group,
        /// or <tt>-1</tt> if the match was successful
        /// but the group itself did not match anything
        ///</returns>
        int End(int group);

        /// <summary>
        /// Returns the input subsequence matched by the previous match.
        /// <p> For a matcher <i>m</i> with input sequence <i>s</i>,
        /// the expressions <i>m.</i><tt>group()</tt> and
        /// <i>s.</i><tt>substring(</tt><i>m.</i><tt>start(),</tt>&nbsp;<i>m.</i><tt>end())</tt>
        /// are equivalent.  </p>
        /// <p> Note that some patterns, for example <tt>a</tt>, match the empty
        /// string.  This method will return the empty string when the pattern
        /// successfully matches the empty string in the input.  </p>
        /// </summary>
        /// <returns> The (possibly empty) subsequence matched by the previous match,
        /// in string form
        /// </returns>
        string Group();

        /// <summary>
        /// Returns the input subsequence captured by the given group during the
        /// previous match operation.
        /// <p> For a matcher <i>m</i>, input sequence <i>s</i>, and group index
        /// <i>g</i>, the expressions <i>m.</i><tt>group(</tt><i>g</i><tt>)</tt> and
        /// <i>s.</i><tt>substring(</tt><i>m.</i><tt>start(</tt><i>g</i><tt>),</tt>&nbsp;<i>m.</i><tt>end(</tt><i>g</i><tt>))</tt>
        /// are equivalent.  </p>
        /// <p> <a href="Pattern.html#cg">Capturing groups</a> are indexed from left
        /// to right, starting at one.  Group zero denotes the entire pattern, so
        /// the expression <tt>m.group(0)</tt> is equivalent to <tt>m.group()</tt>.
        /// </p>
        /// <p> If the match was successful but the group specified failed to match
        /// any part of the input sequence, then <tt>null</tt> is returned. Note
        /// that some groups, for example <tt>(a)</tt>, match the empty string.
        /// This method will return the empty string when such a group successfully
        /// matches the empty string in the input.  </p>
        /// </summary>
        /// <param name="group">
        /// The index of a capturing group in this matcher's pattern
        /// </param>
        /// <returns>
        /// The (possibly empty) subsequence captured by the group
        /// during the previous match, or <tt>null</tt> if the group
        /// failed to match part of the input
        /// </returns>
        string Group(int group);

        /// <summary>
        /// Returns the number of capturing groups in this match result's pattern.
        /// </summary>
        /// <p>
        /// Group zero denotes the entire pattern by convention. It is not
        /// included in this count.
        /// </p>
        /// <p>
        /// Any non-negative integer smaller than or equal to the value
        /// returned by this method is guaranteed to be a valid group index for
        /// this matcher. 
        /// </p>
        /// <returns>
        /// The number of capturing groups in this matcher's pattern
        /// </returns>
        int GroupCount();
    }
}