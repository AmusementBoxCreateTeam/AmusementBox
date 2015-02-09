<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Index extends CI_Controller {

    public function __construct() {
        parent::__construct();
        #$this->output->enable_profiler(TRUE);
        $this->load->model('logins');
        $this->load->library('logined');
    }

    public function index() {
        $data = '';
        $is_try = $this->session->userdata('is_try');
        $user_id = $this->session->userdata('login_id');
        if (empty($user_id)) {
            if ($is_try === TRUE) {
                $data['error'] = '<p class="error">※IDもしくはPasswordが間違っています。</p>';
            }
            $this->load->view('login', $data);
        } else {
            $this->load->view('index');
        }
    }

    public function login() {
        $this->load->view('login');
    }

    public function try_login() {
        if (empty($_POST)) {
            show_404();
            eixt;
        }
        $this->_validation_login();
        if ($this->form_validation->run() !== false) {
            $result = $this->logins->get_login($this->input->post());
            if (!empty($result)) {
                $session_data = array(
                    'login_id' => $result->login_id
                );
                $session_data += array(
                    'is_try' => TRUE
                );
                $this->session->set_userdata($session_data);
            }else{
                $this->session->sess_destroy();
            }
            redirect('index');
        }else{
            redirect('index');
        }
    }

    public function forgot() {
        $this->load->view('forgot');
    }

    public function reissue() {
        $data = '';
        if (empty($_POST)) {
            show_404();
            exit();
        }
        $this->_validation_forgot();
        if ($this->form_validation->run() !== false) {
            $result = $this->logins->get_forgot($this->input->post());
            if ($result !== 0) {
                $password = substr(base_convert(md5(uniqid()), 16, 36), 0, 8);
                $is_send = $this->_send_email($result->login_id, $result->email, $password);
                if ($is_send === false) {
                    //送信失敗
                    $data['error'] = '<p class="error">※メールが正常にされませんでした。</p>';
                } else {
                    //送信成功
                    $this->logins->update_password($this->input->post('id'), $password);
                }
                $this->load->view('reissue_comp', $data);
            } else {
                $data['error'] = '<p class="error">※IDもしくはemailが間違っています。</p>';
                $this->load->view('forgot', $data);
            }
        } else {
            $this->load->view('forgot');
        }
    }

    public function logout() {
        $this->session->sess_destroy();
        redirect('index');
    }

    private function _validation_login() {
        $this->form_validation->set_rules('id', 'Login Id', 'required|min_length[4]|max_length[16]');
        $this->form_validation->set_rules('password', 'Password', 'required|min_length[4]|max_length[16]');

        $this->form_validation->set_error_delimiters('<p class="error">※', '</p>');
    }

    private function _validation_forgot() {
        $this->form_validation->set_rules('id', 'Login Id', 'requiredmin_length[4]|max_length[16]');
        $this->form_validation->set_rules('email', 'email', 'required|valid_email|max_length[100]');

        $this->form_validation->set_error_delimiters('<p class="error">※', '</p>');
    }

    private function _send_email($id, $email, $password) {
        $this->load->library('email');

        $this->email->set_newline("\r\n");

        $this->email->from('amusement_square_test@gmail.com', 'Square管理部');
        $this->email->to($email);

        $this->email->subject('パスワードの再発行');
        $this->email->message(
                $id . " 様" .
                "\n\n" .
                '新しいパスワード：' . $password .
                "\n\n" .
                'このメールに心当りがない方はこちらまでご連絡ください。' .
                "\n\n" .
                'AmusementSquare管理部' .
                "\n" .
                'amusement_square_test@gmail.com'
        );

        return $this->email->send();
    }

}
