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
        },
        isCollapsed: function () {
            var sidebar = document.querySelector('.sidebar');
            return sidebar ? sidebar.classList.contains('collapsed') : false;
        },
        collapse: function () {
            var sidebar = document.querySelector('.sidebar');
            var mainContent = document.querySelector('.main-content');
            var header = document.querySelector('.app-header');
            if (sidebar) {
                sidebar.classList.add('collapsed');
            }
            if (mainContent) {
                mainContent.classList.add('sidebar-collapsed');
            }
            if (header) {
                header.classList.add('sidebar-collapsed');
            }
            localStorage.setItem('beanshare-sidebar-collapsed', 'true');
        },
        expand: function () {
            var sidebar = document.querySelector('.sidebar');
            var mainContent = document.querySelector('.main-content');
            var header = document.querySelector('.app-header');
            if (sidebar) {
                sidebar.classList.remove('collapsed');
            }
            if (mainContent) {
                mainContent.classList.remove('sidebar-collapsed');
            }
            if (header) {
                header.classList.remove('sidebar-collapsed');
            }
            document.documentElement.classList.remove('sidebar-start-collapsed');
            localStorage.setItem('beanshare-sidebar-collapsed', 'false');
        },
        toggle: function () {
            if (window.beanshare.sidebar.isCollapsed()) {
                window.beanshare.sidebar.expand();
            } else {
                window.beanshare.sidebar.collapse();
            }
        },
        restoreState: function () {
            var saved = localStorage.getItem('beanshare-sidebar-collapsed');
            if (saved === 'true') {
                window.beanshare.sidebar.collapse();
            }
        }
    },
    clickElement: function (id) {
        var el = document.getElementById(id);
        if (el) el.click();
    },
    downloadCsv: function (filename, content) {
        var blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};
