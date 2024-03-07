using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    [DebuggerDisplay("{Title} - {MatchPercentage}")]
    internal class TitleMatch
    {
        private static readonly char[] separator = new[] { ' ' };

        public string Title { get; set; } = "";
        public double MatchPercentage { get; set; }

        public void CalculateMatchPercentage(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                MatchPercentage = 0;
                return;
            }

            var inputWords = userInput.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var titleWords = Title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Direct comparison checks
            if (string.Equals(userInput.Trim(), Title, StringComparison.OrdinalIgnoreCase))
            {
                // Exact match gets the highest score
                MatchPercentage = 100;
                return;
            }

            double matchScore = 0;
            int lastFoundIndex = -1; // Track the last found index to ensure word order is considered

            // Iterate over each word in the input and check against the title words
            foreach (var inputWord in inputWords)
            {
                bool wordFound = false;

                for (int i = 0; i < titleWords.Length; i++)
                {
                    if (titleWords[i].StartsWith(inputWord, StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if the words are in the correct order
                        if (i > lastFoundIndex)
                        {
                            matchScore += 1; // Increment match score
                            lastFoundIndex = i; // Update the last found index
                            wordFound = true;
                            break; // Move to the next input word
                        }
                    }
                }

                // Adjust the match score for partial matches if the full word isn't found
                if (!wordFound)
                {
                    for (int i = Math.Max(0, lastFoundIndex); i < titleWords.Length; i++)
                    {
                        if (titleWords[i].Contains(inputWord, StringComparison.OrdinalIgnoreCase))
                        {
                            matchScore += 0.5; // Partial matches score less
                            break; // Partial match found, no need to continue checking
                        }
                    }
                }
            }

            // Calculate final match percentage
            // Max possible score equals the number of input words
            double maxScore = inputWords.Length;
            double matchPercentage = (matchScore / maxScore) * 99;

            MatchPercentage = matchPercentage;
        }
    }
}
