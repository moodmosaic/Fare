VAT Information Exchange System (VIES) Validator
===================

VAT Information Exchange System (VIES) is an electronic means of transmitting information relating to VAT-registration (i.e., validity of VAT-numbers) of companies registered in EU.

Development environment
-----------------------

* Microsoft Visual Studio 2010
  * .NET Framework 4.0
* ReSharper 6.0
* StyleCop 4.7
* AutoFixture 2.9
  * xUnit.net data theories
* xUnit.net 1.9

Design/Architecture
-------------------

* Included an implementation of the Circuit Breaker stability pattern. (M. Nygard. Release It!: Design and Deploy Production-Ready Software. The Pragmatic Bookshelf, 2007.)
* Included a decorator which wraps the VIES client and applies the Circuit Breaker.
* Written using TDD.

<i>Based on current version <i>(2012/02/26)<i/> of the VIES VAT Web Service. (http://ec.europa.eu/taxation_customs/vies/checkVatService.wsdl)</i>