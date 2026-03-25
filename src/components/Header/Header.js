import React from "react";
import "./HeaderStyles/HeaderStyle.css"

function Header(){
    return (
    <>
        <div className="header">
            <div>
                <h2>Logo</h2>
            </div>
            <div>
                <h1>School Name</h1>
            </div>
            
            <div>
                <a href="/login" className="sign-in-href"><h1>Вход</h1></a>
            </div>
        </div>
    </>
    );
}

export default Header;