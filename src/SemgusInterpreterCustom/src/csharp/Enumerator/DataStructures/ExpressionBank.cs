using System.Collections.Generic;
using System.Linq;
using Semgus.Interpreter;
using Semgus.Syntax;
using Semgus.Util;

namespace Semgus.Enumerator {
    public class ExpressionBank {
        private readonly AutoDict<Nonterminal, DictOfList<int, IDSLSyntaxNode>> _dict
            = new AutoDict<Nonterminal, DictOfList<int, IDSLSyntaxNode>>(_ => new DictOfList<int, IDSLSyntaxNode>());

        public int Size => _dict.Values.Select(k => k.Values.Select(v => v.Count).Sum()).Sum();

        public void Add(Nonterminal nt, int cost, IDSLSyntaxNode expr) {
            _dict.SafeGet(nt).SafeGetCollection(cost).Add(expr);
        }

        public IReadOnlyList<DictOfList<int, IDSLSyntaxNode>> GetCandidateSets(IReadOnlyList<TermInfo> slotTerms) {
            int n = slotTerms.Count;
            DictOfList<int, IDSLSyntaxNode>[] array = new DictOfList<int, IDSLSyntaxNode>[n];
            for (int i = 0; i < n; i++) {
                array[i] = _dict.SafeGet(slotTerms[i].Nonterminal);
            }
            return array;
        }
    }
}