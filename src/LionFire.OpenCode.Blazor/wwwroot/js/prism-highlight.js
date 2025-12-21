// Prism.js highlighting helper for Blazor
window.PrismHighlight = {
    // Highlight all code blocks on the page
    highlightAll: function () {
        if (typeof Prism !== 'undefined') {
            Prism.highlightAll();
        }
    },

    // Highlight a specific element by ID or reference
    highlightElement: function (element) {
        if (typeof Prism !== 'undefined' && element) {
            Prism.highlightElement(element);
        }
    },

    // Highlight all elements within a container
    highlightAllUnder: function (container) {
        if (typeof Prism !== 'undefined' && container) {
            Prism.highlightAllUnder(container);
        }
    },

    // Map common language names to Prism language classes
    getLanguageClass: function (language) {
        if (!language) return 'language-plaintext';

        const languageMap = {
            'cs': 'language-csharp',
            'c#': 'language-csharp',
            'csharp': 'language-csharp',
            'ts': 'language-typescript',
            'typescript': 'language-typescript',
            'js': 'language-javascript',
            'javascript': 'language-javascript',
            'jsx': 'language-jsx',
            'tsx': 'language-tsx',
            'py': 'language-python',
            'python': 'language-python',
            'json': 'language-json',
            'yaml': 'language-yaml',
            'yml': 'language-yaml',
            'bash': 'language-bash',
            'sh': 'language-bash',
            'shell': 'language-bash',
            'sql': 'language-sql',
            'css': 'language-css',
            'html': 'language-markup',
            'xml': 'language-markup',
            'markup': 'language-markup',
            'rust': 'language-rust',
            'rs': 'language-rust',
            'go': 'language-go',
            'golang': 'language-go',
            'diff': 'language-diff',
            'md': 'language-markdown',
            'markdown': 'language-markdown'
        };

        const normalizedLang = language.toLowerCase().trim();
        return languageMap[normalizedLang] || `language-${normalizedLang}`;
    }
};
