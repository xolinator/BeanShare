window.beanshare = {
    isMobile: function () {
        return window.innerWidth <= 768;
    },
    sidebar: {
        isOpen: function () {
            var sidebar = document.querySelector('.sidebar');
            return sidebar ? sidebar.classList.contains('open') : false;
        },
        open: function () {
            var sidebar = document.querySelector('.sidebar');
            var backdrop = document.querySelector('.sidebar-backdrop');
            if (sidebar) {
                sidebar.classList.add('open', 'mobile');
                sidebar.style.transform = '';
                sidebar.style.pointerEvents = '';
            }
            if (backdrop) {
                backdrop.style.display = 'block';
                backdrop.onclick = function () {
                    window.beanshare.sidebar.close();
                };
            }
        },
        close: function () {
            var sidebar = document.querySelector('.sidebar');
            var backdrop = document.querySelector('.sidebar-backdrop');
            if (sidebar) {
                sidebar.classList.remove('open');
                sidebar.style.transform = '';
                sidebar.style.pointerEvents = '';
            }
            if (backdrop) {
                backdrop.style.display = 'none';
                backdrop.onclick = null;
            }
        }
    },
    clickElement: function (id) {
        var el = document.getElementById(id);
        if (el) el.click();
    }
};
