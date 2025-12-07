import React, { useEffect, useState } from "react";
import { Card, Table, Button, message } from "antd";
import api from "../../api/axios";

const BuffetPurchases: React.FC = () => {
    const [purchases, setPurchases] = useState<any[]>([]);
    const [items, setItems] = useState<any[]>([]);

    const load = async () => {
        const r = await api.get("/buffet/my-purchases");
        setPurchases(r.data || []);
        const i = await api.get("/buffet/items");
        setItems(i.data || []);
    };

    useEffect(() => { load(); }, []);

    const buy = async (itemId: string) => {
        try {
            const res = await api.post("/buffet/purchase", { itemId });
            const { paymentId, redirect } = res.data;
            // if redirect to gateway, go there:
            if (redirect && redirect !== "offline") {
                window.location.href = redirect;
            } else {
                message.success("خرید ثبت شد");
                load();
            }
        } catch (err: any) { message.error("خطا"); }
    };

    return (
        <div>
            <Card title="آیتم‌های بوفه">
                <Table dataSource={items} rowKey="id" columns={[
                    { title: "نام", dataIndex: "name" },
                    { title: "قیمت", dataIndex: "price" },
                    { title: "عملیات", render: (_: any, r: any) => <Button onClick={() => buy(r.id)}>خرید</Button> }
                ]} />
            </Card>

            <Card title="خریدهای من" style={{ marginTop: 12 }}>
                <Table dataSource={purchases} rowKey="id" columns={[
                    { title: "آیتم", dataIndex: "itemName" },
                    { title: "مبلغ", dataIndex: "amount" },
                    { title: "پرداخت", dataIndex: "isPaid", render: (v: boolean) => v ? "پرداخت شده" : "نپرداخته" }
                ]} />
            </Card>
        </div>
    );
};

export default BuffetPurchases;
