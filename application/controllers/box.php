<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Box extends CI_Controller {

    public function __construct() {
        parent::__construct();
        #$this->output->enable_profiler(TRUE);
        $this->config->load('box');
        $this->load->model(array('boxes'));
        $this->logined->logincheck();
    }

    /**
     * 端末ページトップ
     */
    public function index() {
        if (empty($_GET)) {
            $data['address'] = "";
            $data['entry_date_over'] = "";
            $data['entry_date_under'] = "";
        } else {
            $data['address'] = $this->input->get('address', true);
            $data['entry_date_over'] = $this->input->get('entry_date_over', true);
            $data['entry_date_under'] = $this->input->get('entry_date_under', true);
        }
        $search = array('address' => $data['address'],
            'entry_date_over' => $data['entry_date_over'],
            'entry_date_under' => $data['entry_date_under']);
        $data['list'] = $this->boxes->get_list($search);
        $data['prefs'] = $this->get_pref_list();
        $this->load->view('box/index.php', $data);
    }


    /**
     * 端末詳細ページ
     */
    public function detail() {
        if (empty($_GET)) {
            $this->load->view('box/index.php');
        } else {
            $data['box'] = $this->boxes->get_box($this->input->get('id'));
            $this->load->view('box/detail.php', $data);
        }
    }



    /**
     * 端末登録ページ
     */
    public function register() {

        if ($_SERVER["REQUEST_METHOD"] == "POST") {
            $this->set_validation_rules();
            // テスト中につき必ずtrue
            if ($this->form_validation->run() == true || true) {
                $this->boxes->add($this->input->post());
            }
        }
        // $url = 'http://maps.googleapis.com/maps/api/geocode/json?language=ja&latlng=35.681382,139.766084&sensor=true_or_false';
        // $json = "";
        // $cp = curl_init();
        // curl_setopt($cp, CURLOPT_RETURNTRANSFER, 1);
        // curl_setopt($cp, CURLOPT_URL, $url);
        // curl_setopt($cp, CURLOPT_TIMEOUT, 60);
        // $json = curl_exec($cp);
        // curl_close($cp);

        // $obj = json_decode($json, true);
        // echo '<pre>';
        // print_r($obj);
        // echo '</pre>';


        $data['pref_list'] = $this->get_pref_list();
        $this->load->view('box/register.php', $data);

    }


    /**
     * 都道府県リストを返す
     */
    private function get_pref_list() {
        return array ('指定しない',
            '北海道',
            '青森県','岩手県','宮城県','秋田県','山形県','福島県',
            '茨城県','栃木県','群馬県','埼玉県','千葉県','東京都','神奈川県',
            '山梨県','新潟県','富山県','石川県','福井県','長野県','岐阜県','静岡県','愛知県',
            '三重県','滋賀県','京都府','大阪府','兵庫県','奈良県','和歌山県',
            '鳥取県','島根県','岡山県','広島県','山口県',
            '徳島県','香川県','愛媛県','高知県',
            '福岡県','佐賀県','長崎県','熊本県','大分県','宮崎県','鹿児島県','沖縄県');
    }


    /**
     * ページング用リンクを返す
     */
    private function pagination($url, $total_rows) {

        $config['base_url'] = $url;
        $config['total_rows'] = $total_rows;
        $config['per_page'] = $this->config->item('disp_num');
        $config['page_query_string'] = true;
        $config['num_links'] = 2;
        $config['first_link'] = '最初';
        $config['prev_link'] = '&lt;';
        $config['next_link'] = '&gt;';
        $config['last_link'] = '最後';

        $this->pagination->initialize($config);
        return $this->pagination->create_links();
    }


    private function set_validation_rules() {
        $config = array();
        $config = array(
            array(
                'field' => 'pref',
                'label' => '都道府県',
                'rule' => 'trim'
                ),
            array(
                'field' => 'entry_date_over',
                'label' => '登録日',
                'rule' => 'trim'
                ),
            array(
                'field' => 'entry_date_under',
                'label' => '登録日',
                'rule' => 'trim'
                )
            );
        $this->form_validation->set_rules($config);
    } 


}
