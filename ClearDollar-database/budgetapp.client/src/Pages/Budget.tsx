import { useEffect, useState } from 'react';
import { Navbar } from "../Components/Navbar";

export function BudgetPage() {
    const [tags, setTags] = useState();

    useEffect(() => {
        populateTags();
    }, []);

    const contents = tags === undefined
        ? <p className="text-center"><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <div className="d-flex justify-content-center">
            <table className="table mx-auto border-separate border-spacing-x-6" aria-labelledby="tableLabel">
                <thead>
                    <tr>
                        <th className="px-2 text-left">TagID</th>
                        <th className="px-2 text-left">ParentID</th>
                        <th className="px-2 text-left">TagName</th>
                        <th className="px-2 text-left">Amount</th>
                    </tr>
                </thead>
                <tbody>
                    {tags.map((tag: any) =>
                        <tr key={tag.tagId}>
                            <td className="px-2">{tag.tagId}</td>
                            <td className="px-2">{tag.parentTagId}</td>
                            <td className="px-2">{tag.tagName}</td>
                            <td className="px-2">{tag.budgetAmount}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>;

    return (
        <>
            <Navbar />
            <div className="container">
                <h1 id="tableLabel" className="text-center">All Tags</h1>
                <p className="text-center">This component demonstrates fetching data from the server.</p>
                {contents}
            </div>
        </>
    );

    async function populateTags() {
        const response = await fetch('/tags?userId=demo-user');
        if (response.ok) {
            const data = await response.json();
            setTags(data);
        }
    }

}