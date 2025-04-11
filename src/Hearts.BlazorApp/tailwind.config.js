module.exports = {
    content: [
        './**/*.{razor,html}',
        './**/*.{cshtml,html}',
        './**/*.{js,ts}',
    ],
    darkMode: 'selector',
    theme: {
        extend: {
            fontFamily: {
                sans: ['Open Sans', 'sans-serif'],
            },
        },
    },
    safelist: [        
        '-ml-16',
        '-ml-32',
    ],
    plugins: [],
}
