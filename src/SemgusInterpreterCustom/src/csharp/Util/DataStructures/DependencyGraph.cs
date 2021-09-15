using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Util {
    public class DependencyGraph<T> {
        private readonly Dictionary<T, IReadOnlyCollection<T>> _dependencyMap = new Dictionary<T, IReadOnlyCollection<T>>();

        /// <summary>
        /// Get the elements of the graph in order from independent to most deeply dependent
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<T> Sort() {
            var sorted = new List<T>();
            foreach (var node in _dependencyMap.Keys.ToList()) {
                Visit(node, sorted, new HashSet<T>());
            }
            return sorted;
        }

        void Visit(T node, List<T> resolved, HashSet<T> entered) {
            if (resolved.Contains(node)) return;
            if (entered.Contains(node)) throw new Exception("Cyclic dependency (mutual constraints are not permitted at this time)");
            entered.Add(node);

            if (_dependencyMap.TryGetValue(node, out var dependencies)) {
                // Try to resolve all this node's dependencies
                foreach (var dep in dependencies) {
                    if (!resolved.Contains(dep)) {
                        Visit(dep, resolved, entered);
                    }
                }
            } else {
                // Independent node (do nothing)
            }

            // All dependencies are resolved at this point
            resolved.Add(node);
        }

        public void Add(T node, IReadOnlyCollection<T> dependencies) => _dependencyMap.Add(node, dependencies);
    }
}