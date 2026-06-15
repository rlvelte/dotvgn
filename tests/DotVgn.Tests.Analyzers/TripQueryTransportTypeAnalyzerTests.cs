using System.Threading.Tasks;
using DotVgn.Analyzers;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DotVgn.Analyzers.TripQueryTransportTypeAnalyzer, DotVgn.Analyzers.TripQueryTransportTypeCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DotVgn.Tests.Analyzers;

public class TripQueryTransportTypeAnalyzerTests {
    [Fact]
    public async Task ValidTransportTypes_NoDiagnostic() {
        var test = """

                   using System;
                   namespace DotVgn.Client.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Client.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Client.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query1 = new DotVgn.Client.Queries.TripQuery(DotVgn.Client.Models.Enumerations.TransportType.Bus, 123);
                               var query2 = new DotVgn.Client.Queries.TripQuery(DotVgn.Client.Models.Enumerations.TransportType.Tram, 456);
                               var query3 = new DotVgn.Client.Queries.TripQuery(DotVgn.Client.Models.Enumerations.TransportType.UBahn, 789);
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
                   namespace DotVgn.Client.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Client.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Client.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query = new DotVgn.Client.Queries.TripQuery({|#0:DotVgn.Client.Models.Enumerations.TransportType.SBahn|}, 123);
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
                   namespace DotVgn.Client.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Client.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Client.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query = new DotVgn.Client.Queries.TripQuery({|#0:DotVgn.Client.Models.Enumerations.TransportType.RBahn|}, 456);
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
                   namespace DotVgn.Client.Queries {
                       public sealed record TripQuery {
                           public TripQuery(DotVgn.Client.Models.Enumerations.TransportType transportType, int tripNumber) { }
                       }
                   }

                   namespace DotVgn.Client.Models.Enumerations {
                       public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                   }

                   namespace Test {
                       class Program {
                           void Method() {
                               var query = new DotVgn.Client.Queries.TripQuery({|#0:DotVgn.Client.Models.Enumerations.TransportType.SBahn|}, 123);
                           }
                       }
                   }
                   """;

        var fixedCode = """

                        using System;
                        namespace DotVgn.Client.Queries {
                            public sealed record TripQuery {
                                public TripQuery(DotVgn.Client.Models.Enumerations.TransportType transportType, int tripNumber) { }
                            }
                        }

                        namespace DotVgn.Client.Models.Enumerations {
                            public enum TransportType { Bus, Tram, UBahn, SBahn, RBahn }
                        }

                        namespace Test {
                            class Program {
                                void Method() {
                                    var query = new DotVgn.Client.Queries.TripQuery(DotVgn.Client.Models.Enumerations.TransportType.Bus, 123);
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
