using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;

namespace TSqlToolsLib
{
    public class TSqlCommentsFromDescriptionListener : TSqlParserBaseListener
    {
    private static String descriptionCommentBeginSymbol = "$";
    private static String descriptionCommentEndSymbol = "$";

    private CommonTokenStream tokens;
    private TokenStreamRewriter rewriter;

    private Dictionary<String, String> descriptions;
    private String currentTable = "";
    private String currentSchema = "";

    public TSqlCommentsFromDescriptionListener(CommonTokenStream tokens, Dictionary<String, String> descriptions)
    {
        this.tokens = tokens;
        this.descriptions = descriptions;
        this.rewriter = new TokenStreamRewriter(tokens);
    }

    
    public void enterCreate_table(TSqlParser.Create_tableContext ctx)
    {
        if (ctx.table_name() == null) return;
        if (ctx.table_name().schema != null)
        {
            currentSchema = removeBrackets(ctx.table_name().schema.GetText());
        }
        else
        {
            currentSchema = "";
        }

        if (ctx.table_name().table != null)
        {
            currentTable = removeBrackets(ctx.table_name().table.GetText());
        }
        else
        {
            currentTable = "";
        }
    }

    
    public void enterColumn_definition(TSqlParser.Column_definitionContext ctx)
    {
            string columnName = removeBrackets(ctx.id_()[0].GetText());

            int offset = 0;
            if (tokens.Get(ctx.Stop.TokenIndex + 1).Type == TSqlParser.COMMA)
            {
                offset = 1;
            }

            String descriptionKey = String.Join(".", currentSchema, currentTable, columnName);
        if (!descriptions.ContainsKey(descriptionKey)) return;
        var description = descriptions[descriptionKey];

        rewriter.InsertAfter(ctx.Stop.TokenIndex + offset, "\t\t\t/*" + descriptionCommentBeginSymbol + description + descriptionCommentEndSymbol + "*/");
    }

    public String getResultTSql()
    {
        return rewriter.GetText();
    }

    private String removeBrackets(String ins)
    {
        return ins.Replace("[", "").Replace("]", "");
    }
}

}
