using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Object.Api;
using OllamaClient;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeFixContinue
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixContinueCodeFixProvider)), Shared]
    public class CodeFixContinueCodeFixProvider : CodeFixProvider
    {
        private IClient _client = new Client("127.0.0.1", "11434");
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CodeFixContinueAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Apply suggested code",
                    createChangedDocument: c => ApplySuggestionAsync(context.Document, declaration, c),
                    equivalenceKey: "Apply suggested code"),
                diagnostic);

           /* var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);*/
        }

        /*private async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var identifierToken = typeDecl.Identifier;
            var newName = identifierToken.Text.ToUpperInvariant();

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }*/

        private async Task<Document> ApplySuggestionAsync(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var methodText = methodDecl.ToFullString();
            var cursorIndex = methodText.Length; // Assume the cursor is at the end of the method

            var suggestedCode = GetOllamaSuggestion(methodText, cursorIndex);

            var newMethodDecl = SyntaxFactory.MethodDeclaration(methodDecl.AttributeLists, methodDecl.Modifiers, methodDecl.ReturnType,
                methodDecl.ExplicitInterfaceSpecifier, methodDecl.Identifier, methodDecl.TypeParameterList, methodDecl.ParameterList,
                methodDecl.ConstraintClauses, SyntaxFactory.Block(SyntaxFactory.ParseStatement(suggestedCode)), methodDecl.SemicolonToken);

            var newRoot = syntaxRoot.ReplaceNode(methodDecl, newMethodDecl);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }

        private string GetOllamaSuggestion(string code, int index)
        {
            Generate gen = new Generate()
            {
                KeepAlive = 180,
                Model = "starcoder2:3b",

                Prompt = code.Insert(0, "<fim_prefix>").Insert(index, "<fim_middle>"),
                Format = "json"
            };
            List<ResponseGenerate> gens = _client.Generate(gen).Result;
            string result = "";
            for (int i = 0; i < gens.Count; i++)
            {
                result += gens[i].Response;
            }
            return result;
        }

    }
}
