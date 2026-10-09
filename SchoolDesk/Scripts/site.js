'use strict';
document.querySelectorAll('[data-print]').forEach(function(button){button.addEventListener('click',function(){window.print();});});
document.querySelectorAll('[data-confirm]').forEach(function(form){form.addEventListener('submit',function(event){if(!window.confirm(form.getAttribute('data-confirm')))event.preventDefault();});});
document.querySelectorAll('.sidebar nav a').forEach(function(link){if(new URL(link.href).pathname.toLowerCase()===window.location.pathname.toLowerCase())link.classList.add('active');});
