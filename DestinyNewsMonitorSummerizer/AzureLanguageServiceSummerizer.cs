using Azure.AI.TextAnalytics;
using Azure;
using System.Text;

namespace DestinyNewsMonitorSummerizer
{
    public class AzureLanguageServiceSummerizer : AbstractArticleSummerizer
    {
        private readonly string m_languageKey;
        private readonly string m_languageEndPoint;

        public AzureLanguageServiceSummerizer(string languageKey, string languageEndPoint) 
        {
            m_languageKey = languageKey;
            m_languageEndPoint = languageEndPoint;
        }
        public override async Task<StringBuilder> SummerizeText(string text)
        {
            var summarySb = new StringBuilder();
            var batchInput = new List<string> { text };

            var actions = new TextAnalyticsActions
            {                
                AbstractiveSummarizeActions = new List<AbstractiveSummarizeAction>() { new AbstractiveSummarizeAction() }
            };
            var azureKeyCredential = new AzureKeyCredential(m_languageKey);
            var client = new TextAnalyticsClient(new Uri(m_languageEndPoint), azureKeyCredential);
            var operation = await client.StartAnalyzeActionsAsync(batchInput, actions);
            await operation.WaitForCompletionAsync();
            
            // View operation results.
            await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
            {
                var summaryResults = documentsInPage.AbstractiveSummarizeResults;

                foreach (var summaryActionResults in summaryResults)
                {
                    if (summaryActionResults.HasError)
                    {
                        Console.WriteLine($"  Error!");
                        Console.WriteLine($"  Action error code: {summaryActionResults.Error.ErrorCode}.");
                        Console.WriteLine($"  Message: {summaryActionResults.Error.Message}");
                        continue;
                    }

                    foreach (var documentResults in summaryActionResults.DocumentsResults)
                    {
                        if (documentResults.HasError)
                        {
                            Console.WriteLine($"  Error!");
                            Console.WriteLine($"  Document error code: {documentResults.Error.ErrorCode}.");
                            Console.WriteLine($"  Message: {documentResults.Error.Message}");
                            continue;
                        }
                        

                        //foreach (ExtractiveSummarySentence sentence in documentResults.Sentences)
                        foreach (var sentence in documentResults.Summaries)
                        {
                            summarySb.AppendLine(sentence.Text);                            
                        }
                    }
                }
            }

            return summarySb;
        }
    }
}
