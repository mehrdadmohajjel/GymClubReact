import axios from "axios";

const API_URL = import.meta.env.VITE_API_URL || "https://localhost:5001/api";

const api = axios.create({
    baseURL: API_URL,
    headers: { "Content-Type": "application/json" }
});

api.interceptors.request.use((cfg) => {
    const token = localStorage.getItem("accessToken");
    if (token && cfg.headers) cfg.headers["Authorization"] = `Bearer ${token}`;
    return cfg;
});

api.interceptors.response.use((r) => r, async (error) => {
    if (error.response?.status === 401) {
        // try refresh
        const refreshToken = localStorage.getItem("refreshToken");
        if (refreshToken) {
            try {
                const resp = await axios.post(`${API_URL.replace("/api", "")}/api/auth/refresh`, { refreshToken });
                localStorage.setItem("accessToken", resp.data.accessToken);
                localStorage.setItem("refreshToken", resp.data.refreshToken);
                // retry original
                error.config.headers["Authorization"] = `Bearer ${resp.data.accessToken}`;
                return axios(error.config);
            } catch {
                localStorage.removeItem("accessToken");
                localStorage.removeItem("refreshToken");
                window.location.href = "/login";
            }
        }
    }
    return Promise.reject(error);
});

export default api;
