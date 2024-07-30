using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Editor;
using System.Runtime.InteropServices;
namespace ContinueExtension
{
    
    internal class SuggestionCommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget _nextCommandHandler;
        private IWpfTextView _textView;
        private string _suggestion;
        private bool _added;
        private bool _adorned;
        public IOleCommandTarget NextCommandHandler
        {
            get { return _nextCommandHandler; }
            set { _nextCommandHandler = value; }
        }
        public string DisplayName => "SuggestionCommand";

        public CommandState GetCommandState(TypeCharCommandArgs args)
        {
            return CommandState.Unspecified;
        }

        public SuggestionCommandHandler(IWpfTextView textView)
        {
            _textView = textView;
            _adorned = false;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            /*if (!_adorned)
            {
                char typedChar = char.MinValue;
                if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
                {
                    typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
                    if (typedChar.Equals('+'))
                    {
                        
                        _adorned = true;
                    }
                }
            }

            return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);*/
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
            {
                // Insert the suggestion at the current caret position
                using (var edit = _textView.TextBuffer.CreateEdit())
                {
                    _suggestion = CodeSuggestionAdornment.suggestion;
                    edit.Insert(_textView.Caret.Position.BufferPosition, _suggestion);
                    edit.Apply();
                }

                return VSConstants.S_OK;
            }
            return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
    }
}