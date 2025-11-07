'use strict';

(function () {
    const showModal = (modalId, contentId) => {
        const modal = document.getElementById(modalId);
        const content = document.getElementById(contentId);
        if (!modal || !content) return;

        modal.classList.remove('hidden');
        setTimeout(() => {
            content.classList.remove('scale-95', 'opacity-0');
            content.classList.add('scale-100', 'opacity-100');
        }, 10);
    };

    const hideModal = (modalId, contentId) => {
        const modal = document.getElementById(modalId);
        const content = document.getElementById(contentId);
        if (!modal || !content) return;

        content.classList.remove('scale-100', 'opacity-100');
        content.classList.add('scale-95', 'opacity-0');
        setTimeout(() => {
            modal.classList.add('hidden');
        }, 200);
    };

    async function openDetailModal(id) {
        if (!id) return;

        try {
            const res = await fetch(`/UserManagement/Details/${encodeURIComponent(id)}`, {
                credentials: 'same-origin'
            });
            if (!res.ok) throw new Error('Failed to fetch user details.');

            const data = await res.json();
            const assignValue = (inputId, value) => {
                const el = document.getElementById(inputId);
                if (el) el.value = value ?? '';
            };

            assignValue('detailId', data.id ?? data.Id);
            assignValue('detailEmail', data.email ?? data.Email);
            assignValue('detailFullName', data.fullName ?? data.FullName);
            assignValue('detailRole', data.role ?? data.Role);
            assignValue('detailCreatedAt', data.createdAt ?? data.CreatedAt);

            showModal('detailModal', 'detailModalContent');
        } catch (error) {
            console.error(error);
            alert('Error loading user details. Please try again.');
        }
    }

    async function saveUserChanges() {
        const form = document.getElementById('editUserForm');
        if (!form) {
            alert('Edit form not found.');
            return;
        }

        const formData = new FormData(form);

        try {
            const res = await fetch('/UserManagement/Update', {
                method: 'POST',
                body: formData,
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!res.ok) {
                let errText = 'Failed to update user. Server returned ' + res.status;
                try {
                    const errJson = await res.json();
                    if (errJson?.error) errText = errJson.error;
                } catch {
                    /* ignore parse failure */
                }
                throw new Error(errText);
            }

            const result = await res.json();
            if (result?.success) {
                alert('User updated successfully!');
                hideModal('detailModal', 'detailModalContent');
                window.location.reload();
            } else {
                alert(result?.error || 'Failed to update user. Please check your input.');
            }
        } catch (error) {
            console.error(error);
            alert(error.message || 'Error while saving changes.');
        }
    }

    window.openAddModal = () => showModal('addModal', 'addModalContent');
    window.closeAddModal = () => hideModal('addModal', 'addModalContent');
    window.openDetailModal = openDetailModal;
    window.closeDetailModal = () => hideModal('detailModal', 'detailModalContent');
    window.saveUserChanges = saveUserChanges;
})();
