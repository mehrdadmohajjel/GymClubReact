import React, { useEffect, useState } from "react";
import { Card, List, Button } from "antd";
import api from "../../api/axios";

const Dashboard: React.FC = () => {
    const [gyms, setGyms] = useState<any[]>([]);

    useEffect(() => {
        (async () => {
            const res = await api.get("/gyms/pending");
            setGyms(res.data || []);
        })();
    }, []);

    const approve = async (id: string) => {
        await api.post(`/gyms/${id}/approve`);
        setGyms(gyms.filter(g => g.id !== id));
    };

    return (
        <div>
            <Card title="درخواست‌های باشگاه">
                <List dataSource={gyms} renderItem={(g: any) => (
                    <List.Item actions={[<Button onClick={() => approve(g.id)}>تایید</Button>]}>
                        <List.Item.Meta title={g.name} description={g.address} />
                    </List.Item>
                )} />
            </Card>
        </div>
    );
};

export default Dashboard;
