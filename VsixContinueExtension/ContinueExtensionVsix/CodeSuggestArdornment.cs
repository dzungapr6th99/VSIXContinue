using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using Microsoft.VisualStudio.Editor;
using System.Linq;
using OllamaClient;
using Object.Api;
using System.Collections.Generic;

namespace ContinueExtensionVsix
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class CodeSuggestionAdornment : IWpfTextViewCreationListener
    {
        public static string suggestion;
        [Import]
        static Client _client = new Client("127.0.0.1", "11434");
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Export(typeof(AdornmentLayerDefinition)), Name("CodeSuggestionAdornment"), Order(After = PredefinedAdornmentLayers.Selection, Before = Microsoft.VisualStudio.Text.Editor.PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition editorAdornment;

        [Export(typeof(IVsTextViewCreationListener))]
        [Name("Suggestion Command Handler")]
        [TextViewRole(PredefinedTextViewRoles.Editable)]
        [ContentType("code")]
        internal class TextViewCreationListener : IVsTextViewCreationListener
        {
            
            [Import]
            internal IVsEditorAdaptersFactoryService AdapterService = null;

            public void VsTextViewCreated(IVsTextView textViewAdapter)
            {
                IWpfTextView textView = AdapterService.GetWpfTextView(textViewAdapter);
                if (textView == null)
                    return;

                
                IOleCommandTarget next;
                SuggestionCommandHandler handler = new SuggestionCommandHandler(textView);
                textViewAdapter.AddCommandFilter(handler, out next);
                if (next != null)
                {
                    handler.NextCommandHandler = next;
                }
            }
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.LayoutChanged += OnTextViewLayoutChanged;
        }

        private void OnTextViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            IWpfTextView textView = (IWpfTextView)sender;
            ITextSnapshot snapshot = textView.TextSnapshot;
            ITextCaret caret = textView.Caret;
            var line = e.NewOrReformattedLines.SingleOrDefault(l => l.Start.GetContainingLine().LineNumber != null);
            if (line == null) return;
            var geometry = textView.TextViewLines.GetMarkerGeometry(line.Extent);
            if (geometry == null) return;
            int caretPosition = caret.Position.BufferPosition.Position;
            string text = snapshot.GetText();

            // Call your suggestion function
            suggestion = GetOllamaSuggestion(text, caretPosition);

            // Display the suggestion (simple example using a text block)
            var layer = textView.GetAdornmentLayer("CodeSuggestionAdornment");
            layer.RemoveAllAdornments();

            TextBlock suggestionTextBlock = new TextBlock
            {
                Text = suggestion,
                Background = new SolidColorBrush(Colors.Yellow),
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            Canvas.SetLeft(suggestionTextBlock, caret.Left);
            Canvas.SetTop(suggestionTextBlock, caret.Top);

            layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, line.Extent, null, suggestionTextBlock, /*(tag, ui) => suggestionTextBlock =*/ null);
        }

        private string MySuggestion(string text, string index)
        {
            // Implement your suggestion logic here
            // For this example, just return a placeholder text
            return $"Suggestion for position {index}";
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
            List<ResponseGenerate> gens= _client.Generate(gen).Result;
            string result = "";
            for (int i =0; i< gens.Count; i++)
            {
                result += gens[i].Response;
            }
            return result;
        }
    }
}