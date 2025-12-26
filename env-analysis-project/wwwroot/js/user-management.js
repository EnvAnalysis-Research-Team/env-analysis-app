'use strict';

(function () {
    const getAntiForgeryToken = () => {
        const tokenInput =
            document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]') ??
            document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput?.value ?? '';
    };

    const fetchJson = (url, { method = 'GET', body, headers = {} } = {}) => {
        const finalHeaders = {
            'X-Requested-With': 'XMLHttpRequest',
            ...headers
        };

        const token = getAntiForgeryToken();
        if (token) {
            finalHeaders['RequestVerificationToken'] = token;
        }

        return fetch(url, {
            method,
            body,
            headers: finalHeaders,
            credentials: 'same-origin'
        });
    };

    const postJson = (url, payload) =>
        fetchJson(url, {
            method: 'POST',
            body: JSON.stringify(payload),
            headers: { 'Content-Type': 'application/json' }
        });

    const toggleAppModal = (modalId, open) => {
        const modal = document.getElementById(modalId);
        if (!modal) return;
        modal.classList[open ? 'add' : 'remove']('app-modal--open');
        modal.setAttribute('aria-hidden', open ? 'false' : 'true');
    };

    const toggleFilterModal = (open) => toggleAppModal('filterModal', open);
    const showModal = (modalId) => toggleAppModal(modalId, true);
    const hideModal = (modalId) => toggleAppModal(modalId, false);

    const formatDate = (value) => {
        if (!value) return '-';
        try {
            return new Date(value).toLocaleString();
        } catch {
            return value;
        }
    };

    const loadUserDetails = async (id) => {
        if (!id) throw new Error('User identifier is required.');

        const res = await fetchJson(`/UserManagement/Details/${encodeURIComponent(id)}`);
        if (!res.ok) throw new Error('Failed to fetch user details.');

        const data = await res.json();
        return data?.data ?? data;
    };

    async function openEditModal(id) {
        if (!id) return;

        try {
            const payload = await loadUserDetails(id);
            const isDeleted = Boolean(payload?.isDeleted ?? payload?.IsDeleted);
            if (isDeleted) {
                alert('This user has been deleted. Restore the user before editing.');
                return;
            }
            const assignValue = (inputId, value) => {
                const el = document.getElementById(inputId);
                if (el) el.value = value ?? '';
            };

            assignValue('editId', payload.id ?? payload.Id);
            assignValue('editEmail', payload.email ?? payload.Email);
            assignValue('editFullName', payload.fullName ?? payload.FullName);
            assignValue('editRole', payload.role ?? payload.Role);
            assignValue('editCreatedAt', formatDate(payload.createdAt ?? payload.CreatedAt));

            showModal('editModal');
        } catch (error) {
            console.error(error);
            alert(error.message || 'Error loading user details. Please try again.');
        }
    }

    async function saveUserChanges() {
        const form = document.getElementById('editUserForm');
        if (!form) {
            alert('Edit form not found.');
            return;
        }

        const payload = {
            Id: form.querySelector('#editId')?.value ?? '',
            Email: form.querySelector('#editEmail')?.value ?? '',
            FullName: form.querySelector('#editFullName')?.value ?? '',
            Role: form.querySelector('#editRole')?.value ?? ''
        };

        try {
            const res = await postJson('/UserManagement/Update', payload);

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
                hideModal('editModal');
                window.location.reload();
            } else {
                alert(result?.error || 'Failed to update user. Please check your input.');
            }
        } catch (error) {
            console.error(error);
            alert(error.message || 'Error while saving changes.');
        }
    }

    async function createUser() {
        const form = document.getElementById('addUserForm');
        if (!form) {
            alert('Create form not found.');
            return;
        }

        const payload = {
            Email: form.querySelector('#addEmail')?.value ?? '',
            FullName: form.querySelector('#addFullName')?.value ?? '',
            Role: form.querySelector('#addRole')?.value ?? '',
            Password: form.querySelector('#addPassword')?.value ?? ''
        };

        try {
            const res = await postJson('/UserManagement/Create', payload);
            const result = await res.json();

            if (res.ok && result?.success) {
                alert(result.message || 'User created successfully.');
                hideModal('addModal');
                window.location.reload();
                return;
            }

            const errMessage = result?.message || result?.error || 'Failed to create user.';
            throw new Error(
                result?.errors?.length ? `${errMessage}\n- ${result.errors.join('\n- ')}` : errMessage
            );
        } catch (error) {
            console.error(error);
            alert(error.message || 'Error while creating user.');
        }
    }

    async function deleteUser(id) {
        if (!id) return;
        if (!confirm('This will mark the user as deleted. Continue?')) return;

        try {
            const res = await postJson('/UserManagement/Delete', { Id: id });
            const result = await res.json();

            if (res.ok && result?.success) {
                alert(result.message || 'User deleted successfully.');
                window.location.reload();
                return;
            }

            const errMessage = result?.message || result?.error || 'Failed to delete user.';
            throw new Error(
                result?.errors?.length ? `${errMessage}\n- ${result.errors.join('\n- ')}` : errMessage
            );
        } catch (error) {
            console.error(error);
            alert(error.message || 'Error while deleting user.');
        }
    }

    async function restoreUser(id) {
        if (!id) return;
        if (!confirm('Restore this user?')) return;

        try {
            const res = await postJson('/UserManagement/Restore', { Id: id });
            const result = await res.json();

            if (res.ok && result?.success) {
                alert(result.message || 'User restored successfully.');
                window.location.reload();
                return;
            }

            const errMessage = result?.message || result?.error || 'Failed to restore user.';
            throw new Error(
                result?.errors?.length ? `${errMessage}\n- ${result.errors.join('\n- ')}` : errMessage
            );
        } catch (error) {
            console.error(error);
            alert(error.message || 'Error while restoring user.');
        }
    }

    const wireDismissOnBackdrop = (modalId, closeFn) => {
        const modal = document.getElementById(modalId);
        if (!modal) return;
        modal.addEventListener('click', (event) => {
            if (event.target === modal) closeFn();
        });
    };

    wireDismissOnBackdrop('filterModal', () => toggleFilterModal(false));
    wireDismissOnBackdrop('editModal', () => hideModal('editModal'));
    wireDismissOnBackdrop('addModal', () => hideModal('addModal'));

    window.openAddModal = () => showModal('addModal');
    window.closeAddModal = () => hideModal('addModal');
    window.openEditModal = openEditModal;
    window.closeEditModal = () => hideModal('editModal');
    window.saveUserChanges = saveUserChanges;
    window.createUser = createUser;
    window.deleteUser = deleteUser;
    window.restoreUser = restoreUser;
    window.openFilterModal = () => toggleFilterModal(true);
    window.closeFilterModal = () => toggleFilterModal(false);

    const paginationForm = document.getElementById('paginationForm');
    const paginationPageInput = paginationForm?.querySelector('input[name="page"]');
    const paginationPageSizeInput = paginationForm?.querySelector('input[name="pageSize"]');
    const userPageSizeSelect = document.getElementById('userPageSizeSelect');

    window.changeUserPage = (page) => {
        if (!paginationForm || !paginationPageInput) return;
        const target = Math.max(1, Number(page) || 1);
        paginationPageInput.value = target.toString();
        paginationForm.submit();
    };

    userPageSizeSelect?.addEventListener('change', (event) => {
        if (!paginationForm || !paginationPageSizeInput || !paginationPageInput) return;
        const size = Number(event.target.value) || 10;
        paginationPageSizeInput.value = size.toString();
        paginationPageInput.value = '1';
        paginationForm.submit();
    });

    const exportBtn = document.getElementById('exportUsersBtn');
    exportBtn?.addEventListener('click', () => {
        const query = window.location.search || '';
        const url = `/UserManagement/Export${query}`;
        window.open(url, '_blank');
    });
})();
