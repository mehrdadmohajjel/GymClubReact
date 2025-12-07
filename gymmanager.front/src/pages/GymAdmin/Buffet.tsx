import React, { useEffect, useState } from "react";
import { Card, Table, Button, Modal, Form, InputNumber, Input, message } from "antd";
import api from "../../api/axios";

const Buffet: React.FC = () => {
    const [items, setItems] = useState<any[]>([]);
    const [purchases, setPurchases] = useState<any[]>([]);
    const [visible, setVisible] = useState(false);
    const [editing, setEditing] = useState<any>(null);
    const [form] = Form.useForm();

    const load = async () => {
        const res = await api.get("/buffet/items");
        setItems(res.data || []);
        const r2 = await api.get("/buffet/purchases");
        setPurchases(r2.data || []);
    };

    useEffect(() => { load(); }, []);

    const openAdd = () => { setEditing(null); form.resetFields(); setVisible(true); };
    const save = async () => {
        const vals = await form.validateFields();
        try {
            if (editing) await api.put(`/buffet/items/${editing.id}`, vals);
            else await api.post("/buffet/items", vals);
            message.success("ذخیره شد");
            setVisible(false);
            load();
        } catch (err: any) { message.error("خطا"); }
    };

    const del = async (id: string) => { await api.delete(`/buffet/items/${id}`); load(); };

    return (
        <div>
            <Card title="اقلام بوفه" extra={<Button onClick={openAdd}>افزودن</Button>}>
                <Table dataSource={items} rowKey="id" columns={[
                    { title: "نام", dataIndex: "name" },
                    { title: "قیمت", dataIndex: "price" },
                    { title: "وضعیت", dataIndex: "isActive", render: (v: boolean) => v ? "فعال" : "غیرفعال" },
                    { title: "عملیات", render: (_: any, r: any) => <><Button onClick={() => { setEditing(r); form.setFieldsValue(r); setVisible(true); }}>ویرایش</Button><Button danger style={{ marginLeft: 8 }} onClick={() => del(r.id)}>حذف</Button></> }
                ]} />
            </Card>

            <Card title="خریدهای بوفه" style={{ marginTop: 12 }}>
                <Table dataSource={purchases} rowKey="id" columns={[
                    { title: "کاربر", dataIndex: "userId" },
                    { title: "آیتم", dataIndex: "itemName" },
                    { title: "مبلغ", dataIndex: "amount" },
                    { title: "پرداخت", dataIndex: "isPaid", render: (v: boolean) => v ? "پرداخت شده" : "نپرداخته" }
                ]} />
            </Card>

            <Modal title="اقلام بوفه" open={visible} onOk={save} onCancel={() => setVisible(false)}>
                <Form form={form} layout="vertical">
                    <Form.Item name="name" label="نام" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="price" label="قیمت" rules={[{ required: true }]}><InputNumber style={{ width: "100%" }} /></Form.Item>
                    <Form.Item name="isActive" label="فعال" valuePropName="checked"><Input type="checkbox" /></Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default Buffet;
