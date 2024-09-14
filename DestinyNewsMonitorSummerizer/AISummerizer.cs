using Azure.AI.TextAnalytics;
using Azure;
using System.Text;

namespace DestinyNewsMonitorSummerizer
{
    public static class AISummerizer
    {
        public static async Task<StringBuilder> SummerizeText(string text, string languageKey, string languageEndPoint)
        {
            var summarySb = new StringBuilder();
            var batchInput = new List<string>
            {
                text
            };

            var actions = new TextAnalyticsActions
            {
                //ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { new ExtractiveSummarizeAction() }
                AbstractiveSummarizeActions = new List<AbstractiveSummarizeAction>() { new AbstractiveSummarizeAction() }
            };
            var azureKeyCredential = new AzureKeyCredential(languageKey);
            var client = new TextAnalyticsClient(new Uri(languageEndPoint), azureKeyCredential);
            var operation = await client.StartAnalyzeActionsAsync(batchInput, actions);
            await operation.WaitForCompletionAsync();
            // View operation status.
            Console.WriteLine($"AnalyzeActions operation has completed");
            Console.WriteLine();

            Console.WriteLine($"Created On   : {operation.CreatedOn}");
            Console.WriteLine($"Expires On   : {operation.ExpiresOn}");
            Console.WriteLine($"Id           : {operation.Id}");
            Console.WriteLine($"Status       : {operation.Status}");

            Console.WriteLine();
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

                        //Console.WriteLine($"  Extracted the following {documentResults.Sentences.Count} sentence(s):");
                        //Console.WriteLine($"  Extracted the following {documentResults.Summaries.Count} sentence(s):");
                        //Console.WriteLine();

                        //foreach (ExtractiveSummarySentence sentence in documentResults.Sentences)
                        foreach (var sentence in documentResults.Summaries)
                        {
                            summarySb.AppendLine(sentence.Text);
                            //Console.WriteLine($"  Sentence: {sentence.Text}");
                            //Console.WriteLine();
                        }
                    }
                }
            }

            return summarySb;
        }
    }
}
