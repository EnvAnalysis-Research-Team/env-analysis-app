(function () {
    const originalFetch = window.fetch.bind(window);

    const buildHeaders = (existingHeaders, token) => {
        const headers = new Headers(existingHeaders || {});
        if (token && !headers.has('Authorization')) {
            headers.set('Authorization', `Bearer ${token}`);
        }
        return headers;
    };

    window.fetch = function (input, init) {
        const token = window.authState?.accessToken;
        let finalInput = input;
        let finalInit = init ? { ...init } : {};

        if (input instanceof Request) {
            finalInit = {
                method: finalInit.method ?? input.method,
                headers: buildHeaders(finalInit.headers ?? input.headers, token),
                body: finalInit.body ?? input.body,
                mode: finalInit.mode ?? input.mode,
                credentials: finalInit.credentials ?? input.credentials ?? 'include',
                cache: finalInit.cache ?? input.cache,
                redirect: finalInit.redirect ?? input.redirect,
                referrer: finalInit.referrer ?? input.referrer,
                integrity: finalInit.integrity ?? input.integrity,
                keepalive: finalInit.keepalive ?? input.keepalive,
                signal: finalInit.signal ?? input.signal
            };
            finalInput = input.url;
        } else {
            finalInit.headers = buildHeaders(finalInit.headers, token);
            finalInit.credentials = finalInit.credentials ?? 'include';
        }

        return originalFetch(finalInput, finalInit).then(response => {
            if (response.status === 401) {
                window.location.href = '/Identity/Account/Login';
            }
            return response;
        });
    };
})();
