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
/// Provides code fixes for the <see cref="TripQueryTransportTypeAnalyzer"/> diagnostics.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TripQueryTransportTypeCodeFixProvider)), Shared]
public class TripQueryTransportTypeCodeFixProvider : CodeFixProvider {
    /// <summary>
    /// The diagnostics ids this code fix provider can address.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TripQueryTransportTypeAnalyzer.DiagnosticId);

    /// <inheritdoc />
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null || !context.Diagnostics.Any()) {
            return;
        }

        foreach (var diagnostic in context.Diagnostics) {
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            if (node is ArgumentSyntax arg) {
                node = arg.Expression;
            }
            
            if (node is not MemberAccessExpressionSyntax and not IdentifierNameSyntax) {
                continue;
            }
                
            var transportTypes = new[] { "Bus", "Tram", "UBahn" };
            foreach (var type in transportTypes) {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: $"Change to TransportType.{type}",
                        cancellation => ReplaceTransportTypeAsync(context.Document, node, type, cancellation),
                        equivalenceKey: type),
                    diagnostic);
            }
        }
    }

        
    private static async Task<Document> ReplaceTransportTypeAsync(Document document, SyntaxNode node, string newTransportType, CancellationToken cancellation) {
        var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
        if (root == null)
            return document;

        var newNode = node switch {
            MemberAccessExpressionSyntax memberAccess => memberAccess.WithName(SyntaxFactory.IdentifierName(newTransportType).WithTriviaFrom(memberAccess.Name)),
            IdentifierNameSyntax identifier => SyntaxFactory.IdentifierName(newTransportType).WithTriviaFrom(identifier),
            _ => node
        };

        var newRoot = root.ReplaceNode(node, newNode);
        return document.WithSyntaxRoot(newRoot);
    }
}