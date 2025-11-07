'use strict';

(function () {
    // helper: safe text -> avoid HTML injection when inserting server data
    function escapeHtml(s) {
        if (s == null) return '';
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    const MODAL_ANIMATION_MS = 200;
    const toggleModal = (modal, show) => {
        if (!modal) return;
        if (show) {
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            requestAnimationFrame(() => {
                modal.classList.remove('-translate-y-5', 'opacity-0');
                modal.classList.add('translate-y-0', 'opacity-100');
            });
        } else {
            modal.classList.remove('translate-y-0', 'opacity-100');
            modal.classList.add('-translate-y-5', 'opacity-0');
            setTimeout(() => {
                modal.classList.add('hidden');
                modal.classList.remove('flex');
            }, MODAL_ANIMATION_MS);
        }
    };

    // Load source type list from server and render into the small table.
    async function loadSourceTypes() {
        try {
            const res = await fetch('/SourceTypes/GetList', { credentials: 'same-origin' });
            if (!res.ok) throw new Error('Failed to fetch list: ' + res.status);
            const list = await res.json();

            const tbody = document.getElementById('sourceTypeTbody');
            if (!tbody) return;

            tbody.innerHTML = '';

            if (!Array.isArray(list) || list.length === 0) {
                tbody.innerHTML = '<tr><td colspan="2" class="px-2 py-3 text-center text-xs text-gray-400">No source types found.</td></tr>';
                return;
            }

            list.forEach(item => {
                const tr = document.createElement('tr');
                tr.className = 'type-row hover:bg-white cursor-pointer';
                tr.dataset.id = item.sourceTypeID ?? item.SourceTypeID ?? item.SourceTypeId;
                tr.dataset.name = item.sourceTypeName ?? item.SourceTypeName;
                tr.dataset.desc = item.description ?? item.Description;
                tr.dataset.isactive = item.isActive ?? item.IsActive;
                tr.dataset.count = item.count ?? item.Count ?? 0;

                tr.innerHTML = '<td class="px-2 py-2 truncate" title="' + escapeHtml(tr.dataset.name) + '">' + escapeHtml(tr.dataset.name) + '</td>'
                             + '<td class="px-2 py-2 text-center font-medium">' + escapeHtml(tr.dataset.count) + '</td>';

                tr.addEventListener('click', onSourceTypeRowClick);
                tbody.appendChild(tr);
            });
        } catch (err) {
            console.error(err);
        }
    }

    // Click handler: fetch details for selected source type and open modal
    async function onSourceTypeRowClick(ev) {
        // ignore clicks on form controls (none present in row now)
        const tr = ev.currentTarget;
        const id = tr.dataset.id;
        if (!id) return;

        try {
            const res = await fetch('/SourceTypes/Get/' + encodeURIComponent(id), { credentials: 'same-origin' });
            if (!res.ok) {
                alert('Failed to load details (status ' + res.status + ').');
                return;
            }
            const json = await res.json();

            // populate modal
            document.getElementById('edit_SourceTypeID').value = json.sourceTypeID ?? json.SourceTypeID;
            document.getElementById('edit_SourceTypeName').value = json.sourceTypeName ?? json.SourceTypeName ?? '';
            document.getElementById('edit_Description').value = json.description ?? json.Description ?? '';
            document.getElementById('edit_IsActive').checked = !!(json.isActive ?? json.IsActive);

            // show modal
            toggleModal(editTypeModal, true);
        } catch (err) {
            console.error(err);
            alert('Error loading source type details. See console.');
        }
    }

    // ADD form submit (unchanged behavior)
    const addForm = document.getElementById('addTypeForm');
    if (addForm) {
        addForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(addForm);

            try {
                const res = await fetch(addForm.action, {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                let json = {};
                try { json = await res.json(); } catch { json = {}; }

                if (!res.ok || !json.success) {
                    if (json?.errors && Array.isArray(json.errors) && json.errors.length) {
                        alert(json.errors.join(' • '));
                    } else {
                        alert(json?.error || 'Failed to create source type. Check request.');
                    }
                    return;
                }

                alert(`Created "${json.name || 'Source type'}" successfully.`);
                setTimeout(() => location.reload(), 700);
            } catch (err) {
                console.error(err);
                alert('Error saving source type. Check request or server logs.');
            }
        });
    }

    // Edit modal controls
    const editForm = document.getElementById('editTypeForm');
    const closeEdit = document.getElementById('closeEditModal');
    const cancelEdit = document.getElementById('cancelEditBtn');
    const deleteBtn = document.getElementById('deleteTypeBtn');

    if (closeEdit) closeEdit.addEventListener('click', () => toggleModal(editTypeModal, false));
    if (cancelEdit) cancelEdit.addEventListener('click', () => toggleModal(editTypeModal, false));

    if (editForm) {
        editForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('edit_SourceTypeID').value;
            if (!id) return alert('Missing type id.');

            const tokenInput = editForm.querySelector('input[name="__RequestVerificationToken"]');
            const fd = new FormData(editForm);
            fd.append('UpdatedAt', new Date().toISOString());

            try {
                const res = await fetch('/SourceTypes/Edit/' + encodeURIComponent(id), {
                    method: 'POST',
                    body: fd,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!res.ok) {
                    alert('Failed to update source type. Server responded with status ' + res.status);
                    return;
                }

                let json = {};
                try { json = await res.json(); } catch { json = {}; }

                if (json?.success) {
                    alert('Updated successfully.');
                    setTimeout(() => location.reload(), 500);
                } else {
                    // fallback: reload to pick up non-JSON redirect responses
                    setTimeout(() => location.reload(), 200);
                }
            } catch (err) {
                console.error(err);
                alert('Error updating source type. See console.');
            }
        });
    }

    if (deleteBtn) {
        deleteBtn.addEventListener('click', async () => {
            if (!confirm('Are you sure you want to delete this source type?')) return;
            const id = document.getElementById('edit_SourceTypeID').value;
            if (!id) return alert('Missing id.');

            const tokenInput = editForm.querySelector('input[name="__RequestVerificationToken"]');
            const fd = new FormData();
            fd.append('id', id);
            if (tokenInput) fd.append('__RequestVerificationToken', tokenInput.value);

            try {
                const res = await fetch('/SourceTypes/Delete/' + encodeURIComponent(id), {
                    method: 'POST',
                    body: fd,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (res.ok) {
                    try {
                        const json = await res.json();
                        if (json?.success === false) {
                            alert(json?.error || 'Delete failed.');
                            return;
                        }
                    } catch { /* ignore */ }

                    alert('Deleted successfully.');
                    setTimeout(() => location.reload(), 400);
                } else {
                    alert('Delete failed. Server responded with ' + res.status);
                }
            } catch (err) {
                console.error(err);
                alert('Error deleting. See console.');
            }
        });
    }

    // --- ADD SOURCE TYPE MODAL CONTROL  ---
    const addTypeModal = document.getElementById('addTypeModal');
    const editTypeModal = document.getElementById('editTypeModal');
    const openAddTypeBtn = document.getElementById('openAddTypeModalBtn');
    const closeAddTypeBtn = document.getElementById('closeAddTypeModal');
    const cancelAddTypeBtn = document.getElementById('cancelAddTypeBtn');

    if (openAddTypeBtn && addTypeModal) {
        openAddTypeBtn.addEventListener('click', () => toggleModal(addTypeModal, true));
    }

    if (closeAddTypeBtn) {
        closeAddTypeBtn.addEventListener('click', () => toggleModal(addTypeModal, false));
    }

    if (cancelAddTypeBtn) {
        cancelAddTypeBtn.addEventListener('click', () => toggleModal(addTypeModal, false));
    }

    // Đóng modal khi click ra ngoài phần form
    if (addTypeModal) {
        addTypeModal.addEventListener('click', (e) => {
            if (e.target === addTypeModal) {
                toggleModal(addTypeModal, false);
            }
        });
    }

    // --- EMISSION SOURCE MODAL CONTROL ---
    const addSourceModal = document.getElementById('addSourceModal');
    const openAddSourceBtn = document.getElementById('openAddSourceModalBtn');
    const closeAddSourceBtn = document.getElementById('closeAddModal');
    const cancelAddBtn = document.getElementById('cancelAddBtn');

    if (openAddSourceBtn && addSourceModal) {
        openAddSourceBtn.addEventListener('click', () => toggleModal(addSourceModal, true));
    }

    if (closeAddSourceBtn) {
        closeAddSourceBtn.addEventListener('click', () => toggleModal(addSourceModal, false));
    }

    if (cancelAddBtn) {
        cancelAddBtn.addEventListener('click', () => toggleModal(addSourceModal, false));
    }

    // Đóng modal khi click ra ngoài phần form
    if (addSourceModal) {
        addSourceModal.addEventListener('click', (e) => {
            if (e.target === addSourceModal) {
                toggleModal(addSourceModal, false);
            }
        });
    }
    // Submit add emission source via fetch so we stay on this screen.
    const addSourceForm = document.getElementById('addSourceForm');
    if (addSourceForm) {
        addSourceForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(addSourceForm);

            try {
                const res = await fetch(addSourceForm.action, {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                let json = null;
                try {
                    json = await res.json();
                } catch {
                    json = null;
                }

                if (!res.ok || !json?.success) {
                    const errors = Array.isArray(json?.errors) ? json.errors : null;
                    if (errors?.length) {
                        alert(errors.join('\n'));
                    } else {
                        alert(json?.message ?? json?.error ?? 'Failed to create emission source.');
                    }
                    return;
                }

                alert(json.message ?? 'Emission source created successfully.');
                addSourceForm.reset();

                if (addSourceModal) {
                    toggleModal(addSourceModal, false);
                }

                // Reload so the new record and counts show up while staying on the same page.
                setTimeout(() => window.location.reload(), 500);
            } catch (err) {
                console.error(err);
                alert('Could not create emission source. Please try again.');
            }
        });
    }

    // --- EMISSION SOURCE DETAIL MODAL ---

    const editSourceModal = document.getElementById('editSourceModal');
    const closeEditSourceBtn = document.getElementById('closeEditSourceModal');
    const cancelEditSourceBtn = document.getElementById('cancelEditSourceBtn');
    const deleteSourceBtn = document.getElementById('deleteSourceBtn');
    const editSourceForm = document.getElementById('editSourceForm');
    const deleteSourceUrl = (window.sourceManagementConfig && window.sourceManagementConfig.deleteUrl) || '/EmissionSources/Delete';

    const getAntiForgeryToken = () => {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    };

    async function requestSourceDeletion(id) {
        if (!id) throw new Error('Missing emission source identifier.');

        const headers = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };

        const token = getAntiForgeryToken();
        if (token) {
            headers['RequestVerificationToken'] = token;
        }

        const res = await fetch(deleteSourceUrl, {
            method: 'POST',
            credentials: 'same-origin',
            headers,
            body: JSON.stringify({ id: Number(id) })
        });

        const json = await res.json().catch(() => ({}));
        if (!res.ok || !json?.success) {
            throw new Error(json?.error || 'Failed to delete emission source.');
        }

        return json;
    }

    document.querySelectorAll('.delete-source-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = btn.dataset.id;
            const displayName = btn.dataset.name ? `"${btn.dataset.name}"` : 'this emission source';
            if (!id || !confirm(`Are you sure you want to delete ${displayName}?`)) return;

            btn.disabled = true;
            try {
                await requestSourceDeletion(id);
                btn.closest('tr')?.remove();
                alert('Emission source deleted successfully.');
            } catch (err) {
                console.error(err);
                alert(err.message || 'Failed to delete emission source.');
            } finally {
                btn.disabled = false;
            }
        });
    });

    // mở modal khi click icon mắt
    document.querySelectorAll('button[title="View Details"]').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.preventDefault();
            const row = e.target.closest('tr');
            const button = e.target.closest('button');
            const code = button.getAttribute('data-id');

            try {
                function decodeUnicode(str) {
                    try {
                        return JSON.parse('"' + str.replace(/\"/g, '\\"') + '"');
                    } catch (e) {
                        return str;
                    }
                }
                const res = await fetch(`/EmissionSources/Detail/${encodeURIComponent(code)}`, { credentials: 'same-origin' });
                if (!res.ok) throw new Error('Failed to load source detail');
                const data = await res.json();

                    document.getElementById('edit_SourceID').value = data.emissionSourceID ?? data.emissionSourceID ?? ''
                    document.getElementById('edit_SourceCode').value = data.sourceCode ?? data.SourceCode ?? '';
                    document.getElementById('edit_SourceName').value = data.sourceName ?? data.SourceName ?? '';
                    document.getElementById('edit_Latitude').value = data.latitude ?? data.Latitude ?? '';
                    document.getElementById('edit_Longitude').value = data.longitude ?? data.Longitude ?? '';
                    document.getElementById('edit_Location').value = data.location ?? data.Location ?? '';
                    document.getElementById('edit_SourceTypeID').value = data.sourceTypeID ?? data.SourceTypeID ?? '';
                    document.getElementById('edit_IsActiveSource').checked = !!(data.isActive ?? data.IsActive);
                    document.getElementById('emission_sources_des').value = data.description ?? data.Description ?? '';


                const sel = document.getElementById('emission_sources_stid');
                if (sel && data.sourceTypeID)
                    sel.value = data.sourceTypeID ?? data.SourceTypeID;

                    toggleModal(editSourceModal, true);
            } catch (err) {
                console.error(err);
                alert('Error loading emission source details.');
            }
        });
    });

    // đóng modal
    if (closeEditSourceBtn) closeEditSourceBtn.addEventListener('click', () => toggleModal(editSourceModal, false));
    if (cancelEditSourceBtn) cancelEditSourceBtn.addEventListener('click', () => toggleModal(editSourceModal, false));
    if (editSourceModal) {
        editSourceModal.addEventListener('click', e => {
            if (e.target === editSourceModal) toggleModal(editSourceModal, false);
        });
    }

    // submit cập nhật
    if (editSourceForm) {
        editSourceForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('edit_SourceID').value;
            if (!id) return alert('Missing SourceID.');

            const fd = new FormData(editSourceForm);
            try {
                const res = await fetch(`/EmissionSources/Edit/${encodeURIComponent(id)}`, {
                    method: 'POST',
                    body: fd,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                const json = await res.json().catch(() => ({}));
                if (res.ok && json.success) {
                    alert('Updated successfully.');
                    setTimeout(() => location.reload(), 500);
                } else {
                    alert(json.error || 'Failed to update.');
                }
            } catch (err) {
                console.error(err);
                alert('Error updating emission source.');
            }
        });
    }

    // xoá emission source
    if (deleteSourceBtn) {
        deleteSourceBtn.addEventListener('click', async () => {
            if (!confirm('Delete this emission source?')) return;
            const id = document.getElementById('edit_SourceID').value;

            try {
                await requestSourceDeletion(id);
                alert('Deleted successfully.');
                setTimeout(() => location.reload(), 500);
            } catch (err) {
                console.error(err);
                alert(err.message || 'Error deleting emission source.');
            }
        });
    }
    // initial load
    document.addEventListener('DOMContentLoaded', loadSourceTypes);
})();
