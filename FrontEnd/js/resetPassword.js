document.getElementById("resetForm").addEventListener("submit", async function (e) {
  e.preventDefault();

  // جلب القيم من الفورم
  const email = document.getElementById("email").value.trim();
  const code = document.getElementById("code").value.trim();
  const newPassword = document.getElementById("newPassword").value.trim();

  // جلب عناصر عرض الأخطاء
  const emailError = document.getElementById("emailerror");
  const codeError = document.getElementById("codeerror");
  const passwordError = document.getElementById("passwordError");

  // مسح الأخطاء السابقة
  emailError.textContent = "";
  codeError.textContent = "";
  passwordError.textContent = "";
  emailError.classList.remove("text-danger");
  codeError.classList.remove("text-danger");
  passwordError.classList.remove("text-danger");

  let isValid = true;

  // تحقق من البريد الإلكتروني
  if (!email) {
    emailError.textContent = "Email is required.";
    emailError.classList.add("text-danger");
    isValid = false;
  } else {
    // تحقق من صيغة البريد (بسيط)
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      emailError.textContent = "Invalid email format.";
      emailError.classList.add("text-danger");
      isValid = false;
    }
  }

  // تحقق من كود التحقق
  if (!code) {
    codeError.textContent = "Verification code is required.";
    codeError.classList.add("text-danger");
    isValid = false;
  }

  // تحقق من كلمة السر
  if (!newPassword) {
    passwordError.textContent = "Password is required.";
    passwordError.classList.add("text-danger");
    isValid = false;
  } else if (newPassword.length < 8) {
    passwordError.textContent = "Password must be at least 8 characters.";
    passwordError.classList.add("text-danger");
    isValid = false;
  }

  if (!isValid) return;

  try {
    const response = await fetch("http://localhost:5194/api/Auth/reset-password-with-code", {
      method: "POST",
      body: JSON.stringify({ email, code, newPassword }),
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (response.ok) {
      ShowBootstrapToast("Password has been reset. Redirecting to login...", "success");
      setTimeout(() => {
        window.location.href = "login.html";
      }, 2000);
    } else {
      ShowBootstrapToast("Failed to reset password", "danger");
    }
  } catch (error) {
    ShowBootstrapToast("Something went wrong.", "danger");
  }
});

window.ShowBootstrapToast = function (message, type = "info", withButtons = false) {
  const toastId = "custom-toast-" + Date.now();
  const toastHTML = `
    <div id="${toastId}" class="toast align-items-center text-white bg-${type.toLowerCase()} border-0" role="alert" aria-live="assertive" aria-atomic="true">
      <div class="d-flex">
        <div class="toast-body w-100">
          ${message}
          ${
            withButtons
              ? `
            <div class="mt-2 pt-2 border-top d-flex justify-content-end gap-2">
              <button type="button" class="btn btn-light btn-sm" id="btn-add-new">Add Another</button>
              <button type="button" class="btn btn-outline-light btn-sm" id="btn-go-home">Go Home</button>
            </div>`
              : ""
          }
        </div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
      </div>
    </div>
  `;

  let toastContainer = document.getElementById("toast-container");
  if (!toastContainer) {
    toastContainer = document.createElement("div");
    toastContainer.id = "toast-container";
    toastContainer.className = "toast-container position-fixed bottom-0 end-0 p-3";
    document.body.appendChild(toastContainer);
  }

  toastContainer.innerHTML += toastHTML;

  const toastElement = document.getElementById(toastId);
  const toast = new bootstrap.Toast(toastElement, { delay: 7000 });
  toast.show();

  if (withButtons) {
    toastElement.querySelector("#btn-add-new").addEventListener("click", function () {
      document.getElementById("resetForm").reset();
      toast.hide();
    });

    toastElement.querySelector("#btn-go-home").addEventListener("click", function () {
      window.location.replace("../../index.html");
    });
  }

  toastElement.addEventListener("hidden.bs.toast", function () {
    toastElement.remove();
  });
};
