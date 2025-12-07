import React, { useContext } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Auth/Login";
import Register from "./pages/Auth/Register";
import ForgotPassword from "./pages/Auth/ForgotPassword";
import SuperAdminDashboard from "./pages/SuperAdmin/Dashboard";
import GymsManagement from "./pages/SuperAdmin/GymsManagement";
import Members from "./pages/GymAdmin/Members";
import Movements from "./pages/GymAdmin/Movements";
import Buffet from "./pages/GymAdmin/Buffet";
import CreateWorkout from "./pages/Trainer/CreateWorkout";
import AthleteDashboard from "./pages/Athlete/Dashboard";
import MembershipPage from "./pages/Athlete/Membership";
import { AuthContext } from "./contexts/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import LayoutMain from "./components/LayoutMain";

const App: React.FC = () => {
    const { user } = useContext(AuthContext);

    return (
        <LayoutMain>
            <Routes>
                <Route path="/" element={<Navigate to={user ? "/dashboard" : "/login"} />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/forgot" element={<ForgotPassword />} />
                <Route path="/payments/result" element={<PaymentResult />} />
                <Route path="/payments/mockpay" element={<PaymentMock />} />

                <Route path="/dashboard" element={<ProtectedRoute><DashboardSwitch /></ProtectedRoute>} />

                <Route path="*" element={<div>Not Found</div>} />
            </Routes>
        </LayoutMain>
    );
};

const DashboardSwitch = () => {
    const { user } = React.useContext(AuthContext);
    if (!user) return <Navigate to="/login" />;

    if (user.role === "SuperAdmin") return <SuperAdminDashboard />;
    if (user.role === "GymAdmin") return <GymAdminDashboard />;
    if (user.role === "Trainer") return <TrainerDashboard />;
    return <AthleteDashboard />;
};

export default App;
