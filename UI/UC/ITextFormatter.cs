using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace UI.UC
{
    public interface ITextFormatter
    {
        string GetText(FlowDocument document);
        void SetText(FlowDocument document, string text);
        //void SetText(Inline inline, string text);
    }

    /// <summary>
    /// Formats the RichTextBox text as Html
    /// </summary>
    public class HtmlFormatter : ITextFormatter
    {
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return HtmlFromXamlConverter.ConvertXamlToHtml(UTF8Encoding.Default.GetString(ms.ToArray()));
            }
        }

        public void SetText(System.Windows.Documents.FlowDocument document, string text)
        {
            text = HtmlToXamlConverter.ConvertHtmlToXaml(text, false);
            try
            {
                TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    tr.Load(ms, DataFormats.Xaml);
                }
            }
            catch
            {
                //throw new InvalidDataException("data provided is not in the correct Html format.");
            }
        }


    }

    /// <summary>
    /// Formats the RichTextBox text as plain text
    /// </summary>
    public class PlainTextFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            return new TextRange(document.ContentStart, document.ContentEnd).Text;
        }

        public void SetText(FlowDocument document, string text)
        {
            new TextRange(document.ContentStart, document.ContentEnd).Text = text;
        }


    }

    /// <summary>
    /// Formats the RichTextBox text as RTF
    /// </summary>
    public class RtfFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                return ASCIIEncoding.Default.GetString(ms.ToArray());
            }
        }

        public void SetText(FlowDocument document, string text)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text)))
            {
                tr.Load(ms, DataFormats.Rtf);
            }
        }
    }

    /// <summary>
    /// Formats the RichTextBox text as Xaml
    /// </summary>
    public class XamlFormatter : ITextFormatter
    {
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return ASCIIEncoding.Default.GetString(ms.ToArray());
            }
        }

        public void SetText(System.Windows.Documents.FlowDocument document, string text)
        {
            try
            {
                TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text)))
                {
                    tr.Load(ms, DataFormats.Xaml);
                }
            }
            catch
            {
                throw new InvalidDataException("data provided is not in the correct Xaml format.");
            }
        }


    }
}
