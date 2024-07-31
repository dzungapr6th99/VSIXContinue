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
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Document = Microsoft.CodeAnalysis.Document;

namespace CodeFixContinue
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ContinueSuggest)), Shared]
    public class ContinueSuggest : CodeFixProvider
    {
        // TODO: Replace with actual diagnostic id that should trigger this fix.
        public const string DiagnosticId = "ContinueSuggest";
        private IClient _client = new Client("127.0.0.1", "11434");
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
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
        }

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
