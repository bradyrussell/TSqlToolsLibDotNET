using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;

namespace TSqlToolsLib
{
    public class TSqlTools
    {
        public static string GenerateDescriptions(string input)
        {
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new TSqlLexer(new CaseChangingCharStream(stream, true));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            TSqlParser parser = new TSqlParser(tokens);
            parser.BuildParseTree = true;
            var listener = new TSqlDescriptionFromCommentsListener(tokens);
            ParseTreeWalker.Default.Walk(listener, parser.tsql_file());
            var x = listener.GetColumnDescriptions().ToArray();
            return string.Join("\n", x);
        }
    }
}
