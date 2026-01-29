'use strict';

(function () {
    const modal = document.getElementById('trendFilterModal');
    const openBtn = document.getElementById('trendFilterOpen');
    const closeBtn = document.getElementById('trendFilterClose');

    if (!modal || !openBtn || !closeBtn) return;

    const openModal = () => {
        modal.classList.add('app-modal--open');
        modal.setAttribute('aria-hidden', 'false');
    };

    const closeModal = () => {
        modal.classList.remove('app-modal--open');
        modal.setAttribute('aria-hidden', 'true');
    };

    const toggleModal = () => {
        if (modal.classList.contains('app-modal--open')) {
            closeModal();
        } else {
            openModal();
        }
    };

    openBtn.addEventListener('click', toggleModal);
    closeBtn.addEventListener('click', closeModal);
    modal.addEventListener('click', (event) => {
        if (event.target === modal) {
            closeModal();
        }
    });
})();
