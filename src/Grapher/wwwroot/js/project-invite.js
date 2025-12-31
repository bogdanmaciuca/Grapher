(() => {
    const inviteInput = document.getElementById('inviteEmail');
    const suggestions = document.getElementById('inviteSuggestions');
    const inviteForm = document.getElementById('inviteForm');
    const inviteAlert = document.getElementById('inviteAlert');

    function attachSuggestions(input, list) {
        if (!input || !list) return;
        let timer = 0;
        input.addEventListener('input', function () {
            const q = this.value.trim();
            clearTimeout(timer);
            list.innerHTML = '';
            if (!q) return;
            timer = setTimeout(async () => {
                try {
                    const url = `/Projects/SearchUsers?q=${encodeURIComponent(q)}`;
                    const res = await fetch(url);
                    if (!res.ok) return;
                    const items = await res.json();
                    list.innerHTML = items.map(i => `<button type="button" class="list-group-item list-group-item-action suggestion-item">${i}</button>`).join('');
                    list.querySelectorAll('.suggestion-item').forEach(btn => {
                        btn.addEventListener('click', () => {
                            input.value = btn.textContent;
                            list.innerHTML = '';
                        });
                    });
                } catch (e) {
                    // ignore
                }
            }, 250);
        });

        document.addEventListener('click', (e) => {
            if (!list.contains(e.target) && e.target !== input) list.innerHTML = '';
        });
    }

    async function submitInvite(e) {
        e.preventDefault();
        if (!inviteForm) return;
        inviteAlert.classList.remove('d-none', 'alert-success', 'alert-danger');
        inviteAlert.textContent = 'Inviting...';
        const formData = new FormData(inviteForm);
        const body = new URLSearchParams(formData);
        const tokenEl = inviteForm.querySelector('input[name="__RequestVerificationToken"]');
        const headers = tokenEl ? { 'RequestVerificationToken': tokenEl.value } : undefined;

        try {
            const res = await fetch(inviteForm.action, {
                method: 'POST',
                headers,
                body
            });

            if (res.redirected) {
                window.location.href = res.url;
                return;
            }

            if (res.ok) {
                inviteAlert.classList.add('alert-success');
                inviteAlert.textContent = 'Member invited.';
                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('inviteModal'));
                    if (modal) modal.hide();
                }, 700);
                return;
            } else {
                const txt = await res.text();
                inviteAlert.classList.add('alert-danger');
                inviteAlert.textContent = txt || 'Invite failed';
            }
        } catch (err) {
            inviteAlert.classList.add('alert-danger');
            inviteAlert.textContent = 'Invite failed';
        }
    }

    attachSuggestions(inviteInput, suggestions);
    if (inviteForm) inviteForm.addEventListener('submit', submitInvite);
})();
