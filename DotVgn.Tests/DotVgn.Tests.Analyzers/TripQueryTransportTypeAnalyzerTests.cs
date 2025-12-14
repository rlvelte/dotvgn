using System.Threading.Tasks;
using DotVgn.Analyzers;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<DotVgn.Analyzers.TripQueryTransportTypeAnalyzer, DotVgn.Analyzers.TripQueryTransportTypeCodeFixProvider>;

namespace DotVgn.Tests.Analyzers;

// TODO Quick and dirty
public class TripQueryTransportTypeAnalyzerTests {
    [Fact]
    public async Task ValidTransportTypes_NoDiagnostic() {
        var test = """

                   using System;
                   namespace DotVgn.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query1 = new DotVgn.Queries.TripQuery(DotVgn.Models.Enumerations.TransportType.Bus, 123);
                               var query2 = new DotVgn.Queries.TripQuery(DotVgn.Models.Enumerations.TransportType.Tram, 456);
                               var query3 = new DotVgn.Queries.TripQuery(DotVgn.Models.Enumerations.TransportType.UBahn, 789);
                           }
                       }
                   }
                   """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task InvalidTransportType_SBahn_ShowsDiagnostic() {
        var test = """

                   using System;
                   namespace DotVgn.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query = new DotVgn.Queries.TripQuery({|#0:DotVgn.Models.Enumerations.TransportType.SBahn|}, 123);
                           }
                       }
                   }
                   """;

        var expected = VerifyCS.Diagnostic(TripQueryTransportTypeAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("SBahn");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task InvalidTransportType_RBahn_ShowsDiagnostic() {
        var test = """

                   using System;
                   namespace DotVgn.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query = new DotVgn.Queries.TripQuery({|#0:DotVgn.Models.Enumerations.TransportType.RBahn|}, 456);
                           }
                       }
                   }
                   """;

        var expected = VerifyCS.Diagnostic(TripQueryTransportTypeAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("RBahn");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task CodeFix_ChangesToBus() {
        var test = """

                   using System;
                   namespace DotVgn.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query = new DotVgn.Queries.TripQuery({|#0:DotVgn.Models.Enumerations.TransportType.SBahn|}, 123);
                           }
                       }
                   }
                   """;

        var fixedCode = """

                        using System;
                        namespace DotVgn.Queries {
                            public sealed record TripQuery {
                                public TripQuery(DotVgn.Models.Enumerations.TransportType transportType, int tripNumber) { }
                            }
                        }

                        namespace DotVgn.Models.Enumerations {
                            public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                        }

                        namespace Test {
                            class Program {
                                void Method() {
                                    var query = new DotVgn.Queries.TripQuery(DotVgn.Models.Enumerations.TransportType.Bus, 123);
                                }
                            }
                        }
                        """;

        var expected = VerifyCS.Diagnostic(TripQueryTransportTypeAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("SBahn");

        await VerifyCS.VerifyCodeFixAsync(test, expected, fixedCode);
    }
}
