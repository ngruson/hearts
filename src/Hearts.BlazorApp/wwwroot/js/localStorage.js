window.localStorageHelper = {
    saveDarkModeSetting: function (isDarkMode) {
        localStorage.setItem('darkMode', isDarkMode ? 'true' : 'false');
    },
    getDarkModeSetting: function () {
        const value = localStorage.getItem('darkMode');
        return value === 'true';
    },
    toggleDarkMode: function (isDarkMode) {
        if (isDarkMode) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        this.saveDarkModeSetting(isDarkMode);
    },
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    getItem: function (key) {
        return localStorage.getItem(key);
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    }
};
