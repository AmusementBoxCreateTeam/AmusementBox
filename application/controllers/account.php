<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Account extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->load->model('accounts');
        $this->load->library(array('form_validation'));
    }

    public function index() {
        $this->load->view('account/input');
    }

    public function conf() {
        if (empty($_POST)) {
            show_404();
            exit;
        }
        $this->set_validation_rules();
        if ($this->form_validation->run() === FALSE) {
            $this->load->view('account/input');
        } else {
            if ($this->input->post('check_mail') !== "1" && $this->input->post('check_pass') !== "1") {
                $this->load->view('account/input');
            } else {
                $data['post'] = $this->input->post();
                $this->load->view('account/conf', $data);
            }
        }
    }

    public function comp() {
        if (empty($_POST)) {
            show_404();
            exit;
        }
        $this->set_validation_rules();
        if ($this->form_validation->run() === FALSE) {
            $this->load->view('account/input');
        } else {
            if ($this->input->post('check_mail') !== "1" && $this->input->post('check_pass') !== "1") {
                $this->load->view('account/input');
            } else {
                $this->accounts->update($this->input->post());
                $this->load->view('account/comp');
            }
        }
    }

    public function set_validation_rules() {
        $check_mail = $this->input->post('check_mail');
        $check_pass = $this->input->post('check_pass');
        $this->form_validation->set_rules('check_mail', 'required');
        $this->form_validation->set_rules('check_pass', 'required');
        $this->form_validation->set_rules('mail', 'メールアドレス', (($check_mail === "1" ? 'matches[mail_conf]|required' : '')));
        $this->form_validation->set_rules('mail_conf', 'メールアドレス（確認）', (($check_mail === "1" ? 'required' : '')));
        $this->form_validation->set_rules('pass', 'パスワード', (($check_pass === "1" ? 'matches[pass_conf]|required' : '')));
        $this->form_validation->set_rules('pass_conf', 'パスワード（確認）', (($check_pass === "1" ? 'required' : '')));

        $this->form_validation->set_error_delimiters('<p class="error">※', '</p>');
    }

}
