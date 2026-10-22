using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

internal static class Program
{
    private enum BlockKind
    {
        Iff,
        Do,
        Switch
    }

    private sealed class Block
    {
        public BlockKind Kind;
        public int BodyIndent;
    }

    private static int Main()
    {
        try
        {
            string input = Console.In.ReadToEnd();
            Console.Out.Write(Format(input, 4));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("CodeFormat: " + ex.Message);
            return 1;
        }
    }

    private static string Format(string input, int indentSize)
    {
        string[] lines = Regex.Split(input, "\r\n|\n|\r");
        var blocks = new Stack<Block>();
        var output = new List<string>();
        int indent = 0;

        foreach (string originalLine in lines)
        {
            string trimmed = originalLine.Trim();

            if (trimmed.Length == 0)
            {
                AddBlankLine(output);
                continue;
            }

            string keyword = FirstWord(trimmed).ToUpperInvariant();
            bool isIff = keyword == "IFF";
            bool isSingleColonLabel =
                trimmed.StartsWith(":", StringComparison.Ordinal) &&
                !trimmed.StartsWith("::", StringComparison.Ordinal);

            if (isIff || isSingleColonLabel)
                AddBlankLine(output);

            if (keyword == "ENDIFF")
            {
                CloseBlock(blocks, BlockKind.Iff, ref indent);
                output.Add(Indent(indent, indentSize) + trimmed);
                AddBlankLine(output);
                continue;
            }

            if (keyword == "ENDDO")
            {
                CloseBlock(blocks, BlockKind.Do, ref indent);
                output.Add(Indent(indent, indentSize) + trimmed);
                continue;
            }

            if (keyword == "ENDSWITCH")
            {
                CloseBlock(blocks, BlockKind.Switch, ref indent);
                output.Add(Indent(indent, indentSize) + trimmed);
                continue;
            }

            if (keyword == "ELSE" || keyword == "ELSEIFF")
            {
                int iffIndent = FindOpenBlockIndent(blocks, BlockKind.Iff, indent);
                indent = iffIndent;
                output.Add(Indent(indent, indentSize) + trimmed);

                SetTopBodyIndent(blocks, BlockKind.Iff, iffIndent + 1);
                indent = iffIndent + 1;
                continue;
            }

            if (keyword == "CASE" ||
                keyword == "CASEALL" ||
                keyword == "DEFAULT")
            {
                int switchIndent =
                    FindOpenBlockIndent(blocks, BlockKind.Switch, indent);

                output.Add(Indent(switchIndent + 1, indentSize) + trimmed);
                indent = switchIndent + 2;
                continue;
            }

            output.Add(Indent(indent, indentSize) + trimmed);

            if (isIff)
            {
                if (ContainsWord(trimmed, "THEN"))
                {
                    blocks.Push(new Block
                    {
                        Kind = BlockKind.Iff,
                        BodyIndent = indent + 1
                    });
                    indent++;
                }
            }
            else if (keyword == "DO")
            {
                blocks.Push(new Block
                {
                    Kind = BlockKind.Do,
                    BodyIndent = indent + 1
                });
                indent++;
            }
            else if (keyword == "SWITCH")
            {
                blocks.Push(new Block
                {
                    Kind = BlockKind.Switch,
                    BodyIndent = indent + 2
                });
                indent += 2;
            }

            if (keyword == "RETURN")
                AddBlankLine(output);
        }

        while (output.Count > 0 && output[output.Count - 1].Length == 0)
            output.RemoveAt(output.Count - 1);

        return string.Join(Environment.NewLine, output);
    }

    private static void AddBlankLine(List<string> output)
    {
        if (output.Count == 0 || output[output.Count - 1].Length != 0)
            output.Add(string.Empty);
    }

    private static void CloseBlock(
        Stack<Block> blocks,
        BlockKind expected,
        ref int indent)
    {
        if (blocks.Count == 0 || blocks.Peek().Kind != expected)
            return;

        Block block = blocks.Pop();

        indent = block.Kind == BlockKind.Switch
            ? Math.Max(0, block.BodyIndent - 2)
            : Math.Max(0, block.BodyIndent - 1);
    }

    private static int FindOpenBlockIndent(
        Stack<Block> blocks,
        BlockKind kind,
        int fallback)
    {
        foreach (Block block in blocks)
        {
            if (block.Kind == kind)
            {
                return block.Kind == BlockKind.Switch
                    ? Math.Max(0, block.BodyIndent - 2)
                    : Math.Max(0, block.BodyIndent - 1);
            }
        }

        return fallback;
    }

    private static void SetTopBodyIndent(
        Stack<Block> blocks,
        BlockKind kind,
        int bodyIndent)
    {
        foreach (Block block in blocks)
        {
            if (block.Kind == kind)
            {
                block.BodyIndent = bodyIndent;
                return;
            }
        }
    }

    private static string FirstWord(string line)
    {
        int end = 0;

        while (end < line.Length &&
               !char.IsWhiteSpace(line[end]) &&
               line[end] != '(')
        {
            end++;
        }

        return line.Substring(0, end);
    }

    private static bool ContainsWord(string line, string word)
    {
        int index = line.IndexOf(word, StringComparison.OrdinalIgnoreCase);

        while (index >= 0)
        {
            int end = index + word.Length;

            bool leftBoundary =
                index == 0 || !IsWordCharacter(line[index - 1]);

            bool rightBoundary =
                end == line.Length || !IsWordCharacter(line[end]);

            if (leftBoundary && rightBoundary)
                return true;

            index = line.IndexOf(
                word,
                index + 1,
                StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsWordCharacter(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    private static string Indent(int level, int indentSize)
    {
        return new string(' ', Math.Max(0, level) * indentSize);
    }
}
