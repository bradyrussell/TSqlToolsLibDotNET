using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;

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

        public static string GenerateCommentsFromDescriptions(string input)
        {
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new TSqlLexer(new CaseChangingCharStream(stream, true));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            TSqlParser parser = new TSqlParser(tokens);
            parser.BuildParseTree = true;
            var generator = new TSqlCommentsFromDescriptionListener(tokens, ExtractDescriptions(input));
            return generator.getResultTSql();
        }

        public static Dictionary<string,string> ExtractDescriptions(string input)
        {
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new TSqlLexer(new CaseChangingCharStream(stream, true));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            TSqlParser parser = new TSqlParser(tokens);
            parser.BuildParseTree = true;

            var listener = new TSqlExtractDescriptionListener(tokens);
            ParseTreeWalker.Default.Walk(listener, parser.tsql_file());
            return listener.getDescriptions();
        }
    }
}
