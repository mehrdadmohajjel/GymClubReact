import React, { createContext, useEffect, useState } from "react";
import jwtDecode from "jwt-decode";

export type User = {
    id: string;
    role: string;
    gymId?: string;
    nationalCode?: string;
};

type AuthContextType = {
    user?: User | null;
    login: (accessToken: string, refreshToken: string) => void;
    logout: () => void;
};

export const AuthContext = createContext<AuthContextType>({
    login: () => { },
    logout: () => { }
});

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null | undefined>(undefined);

    useEffect(() => {
        const at = localStorage.getItem("accessToken");
        if (at) {
            try {
                const decoded: any = jwtDecode(at);
                setUser({
                    id: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || decoded.nameid || decoded.sub,
                    role: decoded.role,
                    gymId: decoded.gymId
                });
            } catch {
                setUser(null);
            }
        } else {
            setUser(null);
        }
    }, []);

    function login(accessToken: string, refreshToken: string) {
        localStorage.setItem("accessToken", accessToken);
        localStorage.setItem("refreshToken", refreshToken);
        const decoded: any = jwtDecode(accessToken);
        setUser({
            id: decoded.nameid || decoded.sub,
            role: decoded.role,
            gymId: decoded.gymId
        });
    }

    function logout() {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        setUser(null);
        window.location.href = "/login";
    }

    return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>;
};
