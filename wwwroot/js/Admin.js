$(document).ready(function () {
    var codigoUsuario;

    $('#UserTable').DataTable({
        dom: 'Bfrtip',
        buttons: [
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5'
        ]
    });

    $("#btnMensajeExitoso").click(function () {
        $('#modalVentanaExitosa').modal('hide');
        limpiarCamposUsuario();
        location.reload();
    });

    $("#btnMensajeFallido").click(function () {
        $('#modalVentanaFallida').modal('hide');
        location.reload();
    });

    $("#btnAgregarUsuario").click(function () {
        if (validarCamposUsuario()) {

            var userId = document.getElementById("AgregarUserUserId").value;

            if (userId == "") {

                let accountantType;
                let roleId = document.getElementById("AgregarUserRoleId").value;

                if (roleId != 3) {
                    accountantType = null;
                }
                else {
                    accountantType = document.getElementById("AgregarUserAccountantType").value;
                }

                $.ajax({
                    url: '/User/',
                    type: 'POST',
                    data: JSON.stringify({
                        UserId: 0, // Se asigna a posteriori
                        Name: document.getElementById("AgregarUserNombre").value,
                        Username: document.getElementById("AgregarUserUsername").value,
                        Email: document.getElementById("AgregarUserEmail").value,
                        Password: document.getElementById("AgregarUserPassword").value,
                        PasswordSalt: "", // Se asigna a posteriori
                        RoleId: roleId,
                        BossId: document.getElementById("AgregarUserBossId").value,
                        AccountantType: accountantType,
                        Active: document.getElementById("AgregarUserActive").checked
                    }
                    ),
                    async: true,
                    cache: false,
                    contentType: 'application/json',
                    success: function (result) {
                        limpiarCamposUsuario();
                        $('#modalVentanaExitosa').modal('show');
                    },
                    error: function (request, status, err) {
                        $('#modalVentanaFallida').find('.modal-body p').text(request.responseText);

                        $('#modalAgregarUser').modal('hide');
                        $('#modalVentanaFallida').modal('show');
                    }
                });
            }
            else {

                let accountantType;
                let roleId = document.getElementById("AgregarUserRoleId").value;

                if (roleId != 3) {
                    accountantType = null;
                }
                else {
                    accountantType = document.getElementById("AgregarUserAccountantType").value;
                }

                $.ajax({
                    url: '/User/',
                    type: 'PUT',
                    data: JSON.stringify({
                        UserId: document.getElementById("AgregarUserUserId").value,
                        Name: document.getElementById("AgregarUserNombre").value,
                        Username: document.getElementById("AgregarUserUsername").value,
                        Email: document.getElementById("AgregarUserEmail").value,
                        Password: "",
                        PasswordSalt: "",
                        RoleId: roleId,
                        BossId: document.getElementById("AgregarUserBossId").value,
                        AccountantType: accountantType,
                        Active: document.getElementById("AgregarUserActive").checked
                    }
                    ),
                    async: true,
                    cache: false,
                    contentType: 'application/json',
                    success: function (result) {
                        limpiarCamposUsuario();
                        $('#modalVentanaExitosa').modal('show');
                    },
                    error: function (request, status, err) {
                        $('#modalVentanaFallida').find('.modal-body p').text(request.responseText);

                        $('#modalAgregarUser').modal('hide');
                        $('#modalVentanaFallida').modal('show');
                    }
                });
            }

        }
    });

    $("#btnCancelarUsuario").click(function () {
        limpiarCamposUsuario();
    });


    $("a[name='btnEditarUser']").click(function () {
        $("#modalAgregarUser").find('h4').text('Editar Usuario');

        codigoUsuario = $(this).data("id");
        VerDetalleUsuario(codigoUsuario);
    });
    $("#btnAbrirDialogAgregarUsuario").click(function () {
        limpiarCamposUsuario();

        $("#modalAgregarUser").find('h4').text('Agregar Usuario');
        $("#AgregarUserPassword").prop('disabled', false);

        document.getElementById("AgregarUserUserId").value = "";

        $('#modalAgregarUser').modal('show');
    });

    $("#textoBuscarUser").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $("#UserTable tr").filter(function () {
            $(this).toggle($(this).find('td:eq(0), td:eq(1), td:eq(2)').text().replace(/\s+/g, ' ').toLowerCase().indexOf(value) > -1)
        });
    });
});

function limpiarCamposUsuario() {
    $('#modalAgregarUser').modal('hide');

    document.getElementById("AgregarUserUserId").value = "";
    document.getElementById("AgregarUserNombre").value = "";
    document.getElementById("AgregarUserUsername").value = "";
    document.getElementById("AgregarUserEmail").value = "";
    document.getElementById("AgregarUserPassword").value = "";
    document.getElementById("AgregarUserRoleId").value = "";
    document.getElementById("AgregarUserBossId").value = "";
    document.getElementById("AgregarUserAccountantType").value = "";
    document.getElementById("AgregarUserActive").checked = false;

    $("#AgregarUserUserId").css('border', '1px solid #ced4da');
    $("#AgregarUserNombre").css('border', '1px solid #ced4da');
    $("#AgregarUserUsername").css('border', '1px solid #ced4da');
    $("#AgregarUserEmail").css('border', '1px solid #ced4da');
    $("#AgregarUserPassword").css('border', '1px solid #ced4da');
    $("#AgregarUserRoleId").css('border', '1px solid #ced4da');
    $("#AgregarUserBossId").css('border', '1px solid #ced4da');
    $("#AgregarUserAccountantType").css('border', '1px solid #ced4da');
    $("#AgregarUserActive").css('border', '1px solid #ced4da');
}

function validarCamposUsuario() {
    var bandera = true;
    var agregarUserUserId = document.getElementById("AgregarUserUserId").value;
    var agregarUserNombre = document.getElementById("AgregarUserNombre").value;
    var agregarUserUsername = document.getElementById("AgregarUserUsername").value;
    var agregarUserEmail = document.getElementById("AgregarUserEmail").value;
    var agregarUserPassword = document.getElementById("AgregarUserPassword").value;
    var agregarUserRoleId = document.getElementById("AgregarUserRoleId").value;
    var agregarUserBossId = document.getElementById("AgregarUserBossId").value;
    var agregarUserAccountantType = document.getElementById("AgregarUserAccountantType").value;

    if (agregarUserNombre === "") {
        $("#AgregarUserNombre").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#AgregarUserNombre').css('border', '1px solid #ced4da');
    }

    if (agregarUserUsername === "") {
        $("#AgregarUserUsername").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#AgregarUserUsername').css('border', '1px solid #ced4da');
    }

    if (agregarUserEmail === "") {
        $("#AgregarUserEmail").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#AgregarUserEmail').css('border', '1px solid #ced4da');
    }

    if (agregarUserPassword === "" && agregarUserUserId === "") {
        $("#AgregarUserPassword").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#AgregarUserPassword').css('border', '1px solid #ced4da');
    }

    if (agregarUserRoleId === "") {
        $("#AgregarUserRoleId").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#AgregarUserRoleId').css('border', '1px solid #ced4da');
    }

    if (agregarUserBossId === "") {
        $("#AgregarUserBossId").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#AgregarUserBossId').css('border', '1px solid #ced4da');
    }

    if (agregarUserAccountantType === "" && agregarUserRoleId == 3) {
        $("#AgregarUserAccountantType").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#AgregarUserAccountantType').css('border', '1px solid #ced4da');
    }

    return bandera;

}

function VerDetalleUsuario(userId) {

    $.ajax({
        url: '/User?userId=' + userId,
        type: 'GET',
        async: true,
        cache: false,
        // contentType: 'application/json',
        success: function (response) {
            document.getElementById("AgregarUserUserId").value = response.userId;
            document.getElementById("AgregarUserNombre").value = response.name;
            document.getElementById("AgregarUserUsername").value = response.username;
            document.getElementById("AgregarUserEmail").value = response.email;
            document.getElementById("AgregarUserPassword").value = "";
            $("#AgregarUserPassword").prop('disabled', true);
            document.getElementById("AgregarUserRoleId").value = response.roleId;
            document.getElementById("AgregarUserBossId").value = response.bossId;
            document.getElementById("AgregarUserAccountantType").value = response.accountantType;
            document.getElementById("AgregarUserActive").checked = response.active;
        },
        error: function (request, status, err) {
        }
    });

    $('#modalAgregarUser').modal('show');
}

function filterInputNum(event) {
    const inputField = event.target;
    const inputValue = inputField.value;

    // Si el valor ingresado no es un número positivo, establece el valor del campo en vacío
    if (isNaN(inputValue) || Number(inputValue) <= 0) {
        inputField.value = '';
    }
}