<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Statistic extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->load->model(array('statistics'));
    }

    public function index() {
        $data['list'] = $this->statistics->get_rank();
        $this->load->view('statistic/index.php', $data);
    }

    public function rank() {
        if (empty($_POST)) {
            show_404();
            exit();
        }
        $this->configuration();
        if (!$this->form_validation->run()) {
            $data['not_found'] = 'お探しの情報は見つかりませんでした';
        } else {
            $data['list'] = $this->statistics->get_rank($this->input->post());
        }
        $this->load->view('statistic/index.php', $data);
    }

    public function rank_detail($id = '') {
        if (empty($id)) {
            show_404();
            exit();
        }
        //女性
        $data['woman'] = $this->statistics->rank_gender_rate($id,0);
        //男性
        $data['man'] = $this->statistics->rank_gender_rate($id,1);
        $this->load->view('statistic/detail', $data);
    }

    private function configuration() {
        $this->form_validation->set_rules('gender', '性別', 'trim');
        $this->form_validation->set_rules('age', '年代', 'trim');
    }

}
