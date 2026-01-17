    const KEY = "accessToken";

    export const authStore = {
        getAccessToken(): string | null {
            return sessionStorage.getItem(KEY);
        },
        setAccessToken(token: string) {
            sessionStorage.setItem(KEY, token);
        },
        clear() {
            sessionStorage.removeItem(KEY);
        },
    };
