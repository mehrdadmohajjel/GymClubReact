import React, { useContext } from "react";
import { Menu, Dropdown, Button } from "antd";
import { AuthContext } from "../contexts/AuthContext";
import { useNavigate } from "react-router-dom";

const TopMenu: React.FC = () => {
    const { user, logout } = useContext(AuthContext);
    const nav = useNavigate();

    const menu = (
        <Menu>
            <Menu.Item key="profile" onClick={() => nav("/dashboard")}>پروفایل</Menu.Item>
            <Menu.Item key="logout" onClick={() => { logout(); }}>خروج</Menu.Item>
        </Menu>
    );

    return (
        <div>
            {user ? (
                <Dropdown overlay={menu} placement="bottomRight">
                    <Button>{user.role} - {user.id}</Button>
                </Dropdown>
            ) : (
                <div>
                    <Button onClick={() => nav("/login")}>ورود</Button>
                </div>
            )}
        </div>
    );
};

export default TopMenu;
