# Spewspeak

Do you find it too hard to write Internet comments that properly display your intellectual superiority?
Does it take too long to break out the thesaurus and manually exchange words for alternatives appropriate for the scholar that you are?
Look no further than Spewspeak!

Spewspeak is the revolutionary new tool that allows you to quickly boost the reading level of your Internet comments well beyond that of your average Internet plebeian.
With Spewspeak, no one will be able to understand your comments! That's not your problem, they're just too dumb to be able to read them!
With no one able to properly respond, you will have cemented your position as the superior mind in any Internet argument, with minimal effort on your part!

## Tools Used in Development

Spewspeak makes use of a couple different tools/libraries available for its Natural Language Processing functions.

- SharpNL:
This tool was vital to Spewspeak's NLP functionality. SharpNL is an independent C# reimplementation of the Apache OpenNLP library created by Gustavo Knuppe.
I cannot emphasize enough how grateful I am to have had this library available to me. It was a joy to work with, and did everything that I needed it to.
The unit tests included made it very easy to figure out how to interface with the different tools available within it.
Download it here: [SharpNL on GitHub](https://github.com/knuppe/SharpNL)

- WordNet.Net:
This tool was also very important to the function of Spewspeak. WordNet.Net is a library that serves as an C#/.NET interface to the Princeton University WordNet lexical database.
Spewspeak makes use of the WordNet connection to search for synonyms for every word that it attempts to exchange.
WordNet makes this exchange more accurate by only searching for words whose part of speech matches that of the original word.
Download it here: [WordNet.Net on EbSwift.com](http://www.ebswift.com/wordnetnet.html)