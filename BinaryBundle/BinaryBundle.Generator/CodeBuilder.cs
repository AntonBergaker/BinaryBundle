using System.Reflection.PortableExecutable;
using System.Text;

namespace BinaryBundle.Generator;

public class CodeBuilder {
    private StringBuilder _builder;
    private int _indentation;

    public CodeBuilder() {
        _builder = new StringBuilder();
        _indentation = 0;
    }

    public void AddLine(string line) {
        if (line.Contains("\n")) {
            foreach (string str in line.Split('\n')) {
                this.AddLine(str);
            }
            return;
        }
        _builder.AppendLine(new string('\t', _indentation) + line);
    }

    public void AddLines(params string[] lines) {
        foreach (string line in lines) {
            _builder.AppendLine(new string('\t', _indentation) + line);
        }
    }

    public void StartBlock() {
        AddLine("{");
        Indent();
    }

    public void StartBlock(string blockStart) {
        AddLine(blockStart + " {");
        Indent();
    }

    public void EndBlock() {
        Unindent();
        AddLine("}");
    }

    public void Indent() {
        _indentation++;
    }

    public void Unindent() {
        _indentation--;
    }

    public override string ToString() {
        return _builder.ToString();
    }
}
