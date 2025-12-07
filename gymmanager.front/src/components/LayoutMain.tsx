import React from "react";
import { Layout, Menu } from "antd";
import { Link } from "react-router-dom";
import TopMenu from "./TopMenu";

const { Header, Content } = Layout;

const LayoutMain: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    return (
        <Layout style={{ minHeight: "100vh" }}>
            <Header style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <div style={{ color: "#fff", fontWeight: 700 }}>Gym Manager</div>
                <TopMenu />
            </Header>
            <Content style={{ padding: 20 }}>
                {children}
            </Content>
        </Layout>
    );
};

export default LayoutMain;
