window.beanshareTheme = {
    getTheme: function() {
        return localStorage.getItem('beanshare-theme') || 'light';
    },

    setTheme: function(theme) {
        localStorage.setItem('beanshare-theme', theme);
        document.documentElement.setAttribute('data-theme', theme);
        return true;
    },

    init: function() {
        const savedTheme = this.getTheme();
        document.documentElement.setAttribute('data-theme', savedTheme);
        return savedTheme;
    }
};

window.beanshareTheme.init();
