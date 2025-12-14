using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotVgn.Analyzers;

/// <summary>
/// Checks if trip queries contain prohibited transport types.
/// <remarks>DVGN001</remarks>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TripQueryTransportTypeAnalyzer : DiagnosticAnalyzer {
    /// <summary>
    /// The diagnostics identifier for this analyzer.
    /// </summary>
    public const string DiagnosticId = "DVGN001";

    private static readonly LocalizableString Title = "Invalid TransportType for TripQuery";
    private static readonly LocalizableString MessageFormat = "TransportType '{0}' is not allowed for TripQuery. Only Bus, Tram, and UBahn are permitted.";
    private static readonly LocalizableString Description = "TripQuery only supports Bus, Tram, and UBahn as TransportType values.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);


    /// <inheritdoc />
    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeQueryCreation, SyntaxKind.ObjectCreationExpression);
    }

    /// <summary>
    /// Checks if trip queries contain prohibited transport types.
    /// </summary>
    /// <param name="context">The context to run analysis on.</param>
    private static void AnalyzeQueryCreation(SyntaxNodeAnalysisContext context) {
        var creation = (ObjectCreationExpressionSyntax)context.Node;
        
        var typeInfo = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken);
        if (typeInfo.Type == null) {
            return;
        }

        var typeName = typeInfo.Type.ToDisplayString();
        if (!typeName.Contains("TripQuery")) {
            return;
        }
        
        var arguments = creation.ArgumentList?.Arguments;
        if (arguments == null || arguments.Value.Count == 0) {
            return;
        }
        
        var expression = arguments.Value[0].Expression; // TODO: Get by name not by position...
        if (expression is not MemberAccessExpressionSyntax access) {
            return;
        }

        var memberName = access.Name.Identifier.ValueText;
        if (memberName is "Bus" or "Tram" or "UBahn") {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), memberName);
        context.ReportDiagnostic(diagnostic);
    }
}
