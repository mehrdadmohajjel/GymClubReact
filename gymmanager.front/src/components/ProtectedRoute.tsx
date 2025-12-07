import React from "react";
import { Navigate } from "react-router-dom";
import type { JSX } from "react/jsx-runtime";

const ProtectedRoute: React.FC<{ children: JSX.Element }> = ({ children }) => {
    const token = localStorage.getItem("accessToken");
    if (!token) return <Navigate to="/login" />;
    return children;
};

export default ProtectedRoute;
