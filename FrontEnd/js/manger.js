document.addEventListener("DOMContentLoaded", function () {
  const registerCardForm = document.querySelector(".add-manager");
  if (registerCardForm) {
    registerCardForm.addEventListener("submit", async function (event) {
      event.preventDefault();

      const EmailManager = document.getElementById("email").value.trim();
      const Password = document.getElementById("password").value.trim();
      const PhoneNumber = document.getElementById("phone").value.trim();
      const CardID = document.getElementById("card_id").value.trim();

      if (!EmailManager || !Password || !PhoneNumber || !CardID) {
        alert("Data are required!");
        return;
      }

      try {
        const token = localStorage.getItem("token");
        const formData = new FormData();
        formData.append("EmailManager", EmailManager);
        formData.append("Password", Password);
        formData.append("PhoneNumber", PhoneNumber);
        formData.append("CardID", CardID);

        const response = await fetch(
          "hhttp://localhost:5194/api/Auth/AddManager",
          {
            method: "POST",
            body: formData,
            headers: {
              Authorization: `Bearer ${token}`, // Include JWT token in the header
            },
          }
        );

        if (response.ok) {
          const data = await response.text();
          console.log("success add");
          // الانتقال إلى صفحة الهوم
          window.location.href = "manger.html";
        } else {
          const errorData = await response.text();
          console.log("Submit failed:", errorData);
          alert("Submit failed: " + errorData);
        }
      } catch (error) {
        console.error("Submit error:", error);
        alert("An error occurred during Submit  .");
      }
    });
  }
});
