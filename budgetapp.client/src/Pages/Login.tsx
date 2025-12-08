import { Link } from "react-router-dom";


export function LoginPage() {
    return (
        <>
            <div className="flex flex-col items-center justify-center min-h-screen gap-4>">
                <Link to="/dashboard">
                    <button className="px-3 py-1.5 border rounded-lg hover:bg-gray-50">Login</button>
                </Link>
            </div>
 
        </>
    )
}