using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVgn.Analyzers;

/// <summary>
/// 
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TripQueryTransportTypeCodeFixProvider)), Shared]
public class TripQueryTransportTypeCodeFixProvider : CodeFixProvider {
    /// <summary>
    /// The diagnostics id this code fix provider can address.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TripQueryTransportTypeAnalyzer.DiagnosticId);

    /// <inheritdoc />
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        if (root.FindNode(diagnosticSpan) is not MemberAccessExpressionSyntax expression) {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Change to TransportType.Bus",
                cancellation => ReplaceTransportTypeAsync(context.Document, expression, "Bus", cancellation),
                equivalenceKey: "Bus"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Change to TransportType.Tram",
                cancellation => ReplaceTransportTypeAsync(context.Document, expression, "Tram", cancellation),
                equivalenceKey: "Tram"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Change to TransportType.UBahn",
                cancellation => ReplaceTransportTypeAsync(context.Document, expression, "UBahn", cancellation),
                equivalenceKey: "UBahn"),
            diagnostic);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="document"></param>
    /// <param name="memberAccess"></param>
    /// <param name="newTransportType"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    private static async Task<Document> ReplaceTransportTypeAsync(Document document, MemberAccessExpressionSyntax memberAccess, string newTransportType, CancellationToken cancellation) {
        var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
        if (root == null) {
            return document;
        }

        var newMemberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName(newTransportType).WithTriviaFrom(memberAccess.Name));
        var newRoot = root.ReplaceNode(memberAccess, newMemberAccess);

        return document.WithSyntaxRoot(newRoot);
    }
}
